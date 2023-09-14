using PickNPlace.DTO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PickNPlace.Business
{
    public class HkAutoLogic
    {
        public HkAutoLogic()
        {
            this._pallets = new List<HkAutoPallet>();
            this._requestData = new Dictionary<int, PlaceRequestDTO>();
        }

        private IList<HkAutoPallet> _pallets;
        private IDictionary<int, PlaceRequestDTO> _requestData;

        public void SetRequestForPallet(PlaceRequestDTO request, int palletNo)
        {
            var plt = _pallets.FirstOrDefault(d => d.PalletNo == palletNo);
            if (plt == null)
            {
                plt = new HkAutoPallet { PalletNo = palletNo, PalletWidth = 800, PalletHeight = 1200 };
                _pallets.Add(plt);
            }

            if (!_requestData.ContainsKey(palletNo))
                _requestData.Add(palletNo, null);

            _requestData[palletNo] = request;
        }

        public PlaceRequestDTO GetRequestForPallet(int palletNo)
        {
            return _requestData.ContainsKey(palletNo) ? _requestData[palletNo] : null;
        }

        private HkSackSize GetSackSize(int sackType)
        {
            switch (sackType)
            {
                case 1:
                    return new HkSackSize { Width = 37, Height = 56 };
                case 2:
                    return new HkSackSize { Width = 48, Height = 67 };
                case 3:
                    return new HkSackSize { Width = 53, Height = 90 };
                default:
                    break;
            }

            return null;
        }

        private bool CheckIsProperItem(int palletNo, string itemCode)
        {
            var plt = _pallets.FirstOrDefault(d => d.PalletNo == palletNo);
            if (plt != null && plt.Floors != null)
            {
                if (_requestData.ContainsKey(palletNo))
                {
                    int totalCount = 0;
                    foreach (var floor in plt.Floors)
                    {
                        if (floor.Items != null)
                            totalCount += floor.Items.Where(d => d.ItemCode == itemCode).Count();
                    }

                    int requestCount = _requestData[palletNo].Items.Where(d => d.ItemCode == itemCode).Select(d => d.PiecesPerBatch).Sum();

                    return requestCount > totalCount;
                }
            }

            return false;
        }

        private int GetCurrentFloor(int palletNo)
        {
            var plt = _pallets.FirstOrDefault(d => d.PalletNo == palletNo);
            if (plt == null)
                return -1;

            if (plt.Floors == null || plt.Floors.Length == 0)
            {
                plt.Floors = new HkAutoFloor[]
                {
                    new HkAutoFloor
                    {
                        FloorNo = 1,
                        Items = new HkAutoItem[0],
                    }
                };

                return 1;
            }

            var currentFloor = plt.Floors.OrderByDescending(d => d.FloorNo).FirstOrDefault();
            return currentFloor.FloorNo;
        }

        private bool CreateNewFloor(int palletNo, int currentFloor)
        {
            var plt = _pallets.FirstOrDefault(d => d.PalletNo == palletNo);
            if (plt != null)
            {
                var currentList = plt.Floors.OrderBy(d => d.FloorNo).ToList();
                currentList.Add(new HkAutoFloor { FloorNo = currentFloor + 1, Items = new HkAutoItem[0] });
                plt.Floors = currentList.ToArray();

                return true;
            }

            return false;
        }

        private Rectangle SearchForArea(HkSackSize palletSize, HkAutoFloor floor, HkSackSize sackSize, bool isRotated)
        {
            try
            {
                var palletRect = new Rectangle(-3, -3, palletSize.Width, palletSize.Height);

                var flyingItem = new Rectangle(0, 0, 
                    !isRotated ? sackSize.Width : sackSize.Height, 
                    !isRotated ? sackSize.Height : sackSize.Width);

                var existingItemRects = floor.Items.OrderBy(d => d.ItemOrder).Select(d => new Rectangle(
                        d.PlacedX + (d.ItemWidth / 2),
                        d.PlacedY + (d.ItemHeight / 2),
                        d.ItemWidth,
                        d.ItemHeight
                    )).ToArray();

                HkAutoItem lastPlacedItem = null;
                if (floor.Items != null && floor.Items.Length > 0)
                    lastPlacedItem = floor.Items.OrderByDescending(d => d.ItemOrder).FirstOrDefault();

                Rectangle estimatedRect = Rectangle.Empty;
                estimatedRect.Width = flyingItem.Width;
                estimatedRect.Height = flyingItem.Height;

                if (lastPlacedItem != null)
                {
                    var lastPlacedRect = new Rectangle(lastPlacedItem.PlacedX + (lastPlacedItem.ItemWidth / 2),
                        lastPlacedItem.PlacedY + (lastPlacedItem.ItemHeight / 2), lastPlacedItem.ItemWidth, lastPlacedItem.ItemHeight);

                    // ------- 1. estimation ------- //

                    // first shift position to down at right
                    estimatedRect.X = flyingItem.Width;
                    estimatedRect.Y = lastPlacedRect.Y + lastPlacedRect.Height + flyingItem.Height;

                    bool isEstimationValid = false;

                    // validate 1. estimation
                    if (palletRect.Contains(estimatedRect) && !existingItemRects.Any(d => d.IntersectsWith(estimatedRect)))
                        isEstimationValid = true;

                    // out valid 1. estimation
                    if (isEstimationValid)
                        return estimatedRect;

                    // ------- 2. estimation ------- //

                    // second shift position to down at left
                    estimatedRect.X = 0;
                    estimatedRect.Y = lastPlacedRect.Y + lastPlacedRect.Height + flyingItem.Height;

                    // validate 2. estimation
                    if (palletRect.Contains(estimatedRect) && !existingItemRects.Any(d => d.IntersectsWith(estimatedRect)))
                        isEstimationValid = true;

                    // out valid 2. estimation
                    if (isEstimationValid)
                        return estimatedRect;
                }
                else
                {
                    // if floor is empty place right up corner
                    estimatedRect.X = flyingItem.Width;
                    estimatedRect.Y = -0;

                    if (palletRect.Contains(estimatedRect))
                        return estimatedRect;
                }
            }
            catch (Exception)
            {

            }

            return Rectangle.Empty;
        }

        private HkPlacePoint SearchPlacePoint(int palletNo, int floor, HkSackSize sackSize)
        {
            var plt = _pallets.FirstOrDefault(d => d.PalletNo == palletNo);
            if (plt != null && plt.Floors != null)
            {
                var pltFloor = plt.Floors.FirstOrDefault(d => d.FloorNo == floor);
                if (pltFloor != null)
                {
                    bool makeRotate = false;

                    // look at the previous floor to see if a flip is needed
                    var prevFloor = floor > 1 ? plt.Floors.FirstOrDefault(d => d.FloorNo == (floor - 1)) : null;
                    if (prevFloor != null)
                    {
                        var firstPrevItem = prevFloor.Items.OrderBy(d => d.ItemOrder).FirstOrDefault();
                        if (firstPrevItem != null && !firstPrevItem.IsRotated)
                            makeRotate = true;
                    }

                    // search for a proper place by counter-clockwise from right-bottom (robot-frame origin)
                    var properArea = SearchForArea(new HkSackSize { Width = plt.PalletWidth, Height = plt.PalletHeight }, pltFloor, sackSize, makeRotate);
                    if (properArea != Rectangle.Empty)
                    {
                        return new HkPlacePoint
                        {
                            IsRotated = makeRotate,
                            X = plt.PalletWidth - properArea.X - (properArea.Width / 2),
                            Y = plt.PalletHeight - properArea.Y - (properArea.Height / 2),
                        };
                    }
                }
            }

            return null;
        }

        private bool AssignPlacedItem(int palletNo, string itemCode, HkSackSize size, HkPlacePoint placePoint)
        {


            return false;
        }

        public int PlaceAnItem(int palletNo, string itemCode, int sackType)
        {
            try
            {
                var sackSize = GetSackSize(sackType);

                if (sackSize == null)
                    return 2; // sack size couldnt found

                if (!CheckIsProperItem(palletNo, itemCode))
                    return 3; // item is not expected

                var currentFloor = GetCurrentFloor(palletNo);
                var estimatedPlacePoint = SearchPlacePoint(palletNo, currentFloor, sackSize);

                if (estimatedPlacePoint == null)
                {
                    var floorCreationOk = CreateNewFloor(palletNo, currentFloor);
                    if (!floorCreationOk)
                        return 4; // new floor couldnt be created

                    currentFloor++;

                    estimatedPlacePoint = SearchPlacePoint(palletNo, currentFloor, sackSize);
                    if (estimatedPlacePoint == null)
                        return 5; // estimation is not possible
                }

                var assignResult = AssignPlacedItem(palletNo, itemCode, sackSize, estimatedPlacePoint);
                if (!assignResult)
                    return 6; // assignment couldnt be successfull

                return 0; // process was done safely
            }
            catch (Exception)
            {
                return 1; // any not expected error
            }
        }
    }

    class HkSackSize
    {
        public int Width;
        public int Height;
    }

    class HkPlacePoint
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsRotated { get; set; }
    }
}
