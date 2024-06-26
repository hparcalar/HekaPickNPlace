﻿using PickNPlace.DTO;
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

        private readonly int PalletPadding = 3;
        private IList<HkAutoPallet> _pallets;
        private IDictionary<int, PlaceRequestDTO> _requestData;

        public void ClearPallets()
        {
            _pallets.Clear();
            _requestData.Clear();
        }

        public void SetRequestForPallet(PlaceRequestDTO request, int palletNo)
        {
            var plt = _pallets.FirstOrDefault(d => d.PalletNo == palletNo);
            if (plt == null)
            {
                plt = new HkAutoPallet { PalletNo = palletNo, PalletWidth = 1070 + PalletPadding, PalletHeight = 1300 + PalletPadding };
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
                case 1: // middle size
                    return new HkSackSize { Width = 400, Height = 600 };
                case 2: // small size
                    return new HkSackSize { Width = 300, Height = 500 };
                case 3: // large size
                    return new HkSackSize { Width = 415, Height = 650 };
                default:
                    break;
            }

            return null;
        }

        public bool CheckIsProperItem(int palletNo, string itemCode)
        {
            var plt = _pallets.FirstOrDefault(d => d.PalletNo == palletNo);
            if (plt != null)
            {
                if (_requestData.ContainsKey(palletNo))
                {
                    int totalCount = 0;
                    if (plt.Floors != null)
                    {
                        foreach (var floor in plt.Floors)
                        {
                            if (floor.Items != null)
                                totalCount += floor.Items.Where(d => d.ItemCode == itemCode).Count();
                        }
                    }

                    int requestCount = _requestData[palletNo].Items.Where(d => d.ItemCode == itemCode).Select(d => d.PiecesPerBatch).Sum();

                    return requestCount > totalCount;
                }
            }

            return false;
        }

        public int GetCurrentFloor(int palletNo)
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

        private Rectangle SearchForArea(HkSackSize palletSize, HkAutoPallet pallet, HkAutoFloor floor, HkSackSize sackSize, out bool isRotated)
        {
            isRotated = false;

            try
            {
                var palletRect = new Rectangle(-PalletPadding, -PalletPadding, palletSize.Width, palletSize.Height);

                var flyingItem = new Rectangle(0, 0, 
                    sackSize.Width, 
                    sackSize.Height);

                var existingItemRects = floor.Items.OrderBy(d => d.ItemOrder).Select(d => new Rectangle(
                        (pallet.PalletWidth - PalletPadding) - (d.PlacedX + (d.ItemWidth / 2)),
                        (pallet.PalletHeight - PalletPadding) - (d.PlacedY + (d.ItemHeight / 2)),
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
                    var lastPlacedRect = new Rectangle((pallet.PalletWidth - PalletPadding) - (lastPlacedItem.PlacedX + (lastPlacedItem.ItemWidth / 2)),
                        (pallet.PalletHeight - PalletPadding) - (lastPlacedItem.PlacedY + (lastPlacedItem.ItemHeight / 2)), lastPlacedItem.ItemWidth, lastPlacedItem.ItemHeight);

                    // ------- 1. estimation ------- //
                    // -------  ------------ ------- //
                    // -------  ------------ ------- //
                    // -------  ------------ ------- //
                    // -------  ------------ ------- //

                    // first shift position to down at right
                    estimatedRect.X = pallet.PalletWidth - PalletPadding - flyingItem.Width;
                    estimatedRect.Y = lastPlacedRect.X > 0 ? lastPlacedRect.Y + lastPlacedRect.Height : 0;
                    estimatedRect.Width = flyingItem.Width;
                    estimatedRect.Height = flyingItem.Height;

                    // check previous floor to rotate
                    if (floor.FloorNo > 1)
                    {
                        var prevFloor = pallet.Floors.FirstOrDefault(d => d.FloorNo == floor.FloorNo - 1);
                        if (prevFloor != null)
                        {
                            var prevFloorItems = prevFloor.Items.OrderBy(d => d.ItemOrder).Select(d => new
                            {
                                IsRotated = d.IsRotated,
                                Rect = new Rectangle(
                                   (pallet.PalletWidth - PalletPadding) - (d.PlacedX + (d.ItemWidth / 2)),
                                   (pallet.PalletHeight - PalletPadding) - (d.PlacedY + (d.ItemHeight / 2)),
                                   d.ItemWidth,
                                   d.ItemHeight
                                )
                            }).ToArray();

                            if (prevFloorItems.Any(d => d.Rect.X > 0 && d.Rect.IntersectsWith(estimatedRect)))
                            {
                                var intersectingItem = prevFloorItems.FirstOrDefault(d => d.Rect.X > 0 && d.Rect.IntersectsWith(estimatedRect));
                                isRotated = !intersectingItem.IsRotated;
                            }
                        }
                    }

                    // check if it will be rotated
                    if (isRotated)
                    {
                        estimatedRect.X = pallet.PalletWidth - PalletPadding - flyingItem.Height;
                        estimatedRect.Y = lastPlacedRect.X > 0 ? lastPlacedRect.Y + lastPlacedRect.Height : 0;
                        estimatedRect.Width = flyingItem.Height;
                        estimatedRect.Height = flyingItem.Width;
                    }

                    bool isEstimationValid = false;

                    // validate 1. estimation
                    if (palletRect.Contains(estimatedRect) && !existingItemRects.Any(d => d.IntersectsWith(estimatedRect)))
                        isEstimationValid = true;

                    // out valid 1. estimation
                    if (isEstimationValid)
                        return estimatedRect;

                    // ------- 2. estimation ------- //
                    // -------  ------------ ------- //
                    // -------  ------------ ------- //
                    // -------  ------------ ------- //
                    // -------  ------------ ------- //

                    isRotated = true;

                    // second shift position to down at left
                    estimatedRect.X = 0;
                    estimatedRect.Y = lastPlacedRect.X == 0 ? lastPlacedRect.Y + lastPlacedRect.Height : 0;
                    estimatedRect.Width = flyingItem.Height;
                    estimatedRect.Height = flyingItem.Width;

                    // check previous floor to rotate
                    bool isPrevRotated = false;
                    if (floor.FloorNo > 1)
                    {
                        var prevFloor = pallet.Floors.FirstOrDefault(d => d.FloorNo == floor.FloorNo - 1);
                        if (prevFloor != null)
                        {
                            var prevFloorItems = prevFloor.Items.OrderBy(d => d.ItemOrder).Select(d => new
                            {
                                IsRotated = d.IsRotated,
                                Rect = new Rectangle(
                                   (pallet.PalletWidth - PalletPadding) - (d.PlacedX + (d.ItemWidth / 2)),
                                   (pallet.PalletHeight - PalletPadding) - (d.PlacedY + (d.ItemHeight / 2)),
                                   d.ItemWidth,
                                   d.ItemHeight
                                )
                            }).ToArray();

                            if (prevFloorItems.Any(d => d.Rect.X == 0 && d.Rect.IntersectsWith(estimatedRect)))
                            {
                                var intersectingItem = prevFloorItems.FirstOrDefault(d => d.Rect.X == 0 && d.Rect.IntersectsWith(estimatedRect));
                                isPrevRotated = intersectingItem.IsRotated;
                            }
                        }
                    }

                    // check if it will be rotated
                    if (isPrevRotated)
                    {
                        estimatedRect.X = 0;
                        estimatedRect.Y = lastPlacedRect.X == 0 ? lastPlacedRect.Y + lastPlacedRect.Height : 0;
                        estimatedRect.Width = flyingItem.Width;
                        estimatedRect.Height = flyingItem.Height;

                        isRotated = false;
                    }

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
                    estimatedRect.X = pallet.PalletWidth - PalletPadding - flyingItem.Width;
                    estimatedRect.Y = 0;
                    estimatedRect.Width = flyingItem.Width;
                    estimatedRect.Height = flyingItem.Height;

                    // check previous floor to rotate
                    if (floor.FloorNo > 1)
                    {
                        var prevFloor = pallet.Floors.FirstOrDefault(d => d.FloorNo == floor.FloorNo - 1);
                        if (prevFloor != null)
                        {
                            var prevFloorItems = prevFloor.Items.OrderBy(d => d.ItemOrder).Select(d => new
                            {
                                IsRotated = d.IsRotated,
                                Rect = new Rectangle(
                                   (pallet.PalletWidth - PalletPadding) - (d.PlacedX + (d.ItemWidth / 2)),
                                   (pallet.PalletHeight - PalletPadding) - (d.PlacedY + (d.ItemHeight / 2)),
                                   d.ItemWidth,
                                   d.ItemHeight
                                )
                            }).ToArray();

                            if (prevFloorItems.Any(d => d.Rect.X > 0 && d.Rect.IntersectsWith(estimatedRect)))
                            {
                                var intersectingItem = prevFloorItems.FirstOrDefault(d => d.Rect.IntersectsWith(estimatedRect));
                                isRotated = !intersectingItem.IsRotated;
                            }
                        }
                    }

                    // check if it will be rotated
                    if (isRotated)
                    {
                        estimatedRect.X = pallet.PalletWidth - PalletPadding - flyingItem.Height;
                        estimatedRect.Y = 0;
                        estimatedRect.Width = flyingItem.Height;
                        estimatedRect.Height = flyingItem.Width;
                    }

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
                    // search for a proper place by counter-clockwise from right-bottom (robot-frame origin)
                    bool makeRotate = false;

                    var properArea = SearchForArea(new HkSackSize { Width = plt.PalletWidth, Height = plt.PalletHeight }, plt, pltFloor, sackSize, out makeRotate);
                    if (properArea != Rectangle.Empty)
                    {
                        return new HkPlacePoint
                        {
                            IsRotated = makeRotate,
                            X = plt.PalletWidth - PalletPadding - properArea.X - (properArea.Width / 2),
                            Y = plt.PalletHeight - PalletPadding - properArea.Y - (properArea.Height / 2),
                            Width = properArea.Width,
                            Height = properArea.Height,
                        };
                    }
                }
            }

            return null;
        }

        private bool AssignItemAsNext(int palletNo, string itemCode, HkSackSize size, HkPlacePoint placePoint)
        {
            int floorNo = GetCurrentFloor(palletNo);
            var plt = _pallets.FirstOrDefault(d => d.PalletNo == palletNo);
            if (plt != null && plt.Floors != null)
            {
                var currentFloor = plt.Floors.FirstOrDefault(d => d.FloorNo == floorNo);
                if (currentFloor != null)
                {
                    var itemList = currentFloor.Items.ToList();

                    // calculate next item order no
                    int nextOrder = 1;
                    if (itemList.Count() > 0)
                    {
                        nextOrder = itemList.Select(d => d.ItemOrder).OrderByDescending(d => d).First() + 1;
                    }
                    else if (plt.Floors.Length > 1)
                    {
                        var prevFloor = plt.Floors.FirstOrDefault(d => d.FloorNo == floorNo - 1);
                        nextOrder = prevFloor.Items.Select(d => d.ItemOrder).OrderByDescending(d => d).First() + 1;
                    }

                    // add assigned item to current list
                    itemList.Add(new HkAutoItem
                    {
                        IsPlaced = false,
                        IsRotated = placePoint.IsRotated,
                        ItemCode = itemCode,
                        ItemHeight = placePoint.Height,
                        ItemWidth = placePoint.Width,
                        ItemOrder = nextOrder,
                        PlacedX = placePoint.X,
                        PlacedY = placePoint.Y,
                    });

                    currentFloor.Items = itemList.ToArray();

                    return true;
                }
            }

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

                var assignResult = AssignItemAsNext(palletNo, itemCode, sackSize, estimatedPlacePoint);
                if (!assignResult)
                    return 6; // assignment couldnt be successfull

                return 0; // process was done safely
            }
            catch (Exception)
            {
                return 1; // any not expected error
            }
        }

        public bool SignWaitingPlacementIsMade(int palletNo)
        {
            try
            {
                var plt = _pallets.FirstOrDefault(d => d.PalletNo == palletNo);
                if (plt != null)
                {
                    foreach (var floor in plt.Floors)
                    {
                        var waitingItem = floor.Items.FirstOrDefault(d => d.IsPlaced == false);
                        if (waitingItem != null)
                        {
                            waitingItem.IsPlaced = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {

            }

            return false;
        }

        public HkAutoItem GetWaitingItem(int palletNo)
        {
            try
            {
                var plt = _pallets.FirstOrDefault(d => d.PalletNo == palletNo);
                if (plt != null)
                {
                    foreach (var floor in plt.Floors)
                    {
                        var waitingItem = floor.Items.FirstOrDefault(d => d.IsPlaced == false);
                        if (waitingItem != null)
                            return waitingItem;
                    }
                }
            }
            catch (Exception)
            {

            }

            return null;
        }

        public HkAutoPallet GetPalletData(int palletNo)
        {
            return _pallets.FirstOrDefault(d => d.PalletNo == palletNo);
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
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsRotated { get; set; }
    }
}
