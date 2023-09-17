using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PickNPlace.DTO
{
    public class HkAutoPallet
    {
        public int PalletNo { get; set; }
        public int PalletWidth { get; set; }
        public int PalletHeight { get; set; }

        private bool _IsRawMaterial;
        public bool IsRawMaterial
        {
            get
            {
                return _IsRawMaterial;
            }
            set
            {
                var oldValue = _IsRawMaterial;
                _IsRawMaterial = value;

                if (value && !oldValue)
                    CaptureErrorCount = 0;
            }
        }
        public int CaptureErrorCount { get; set; }
        public string RawMaterialCode { get; set; }
        public string PlaceRecipeCode { get; set; }
        public int SackType { get; set; }
        public bool IsEnabled { get; set; }
        public HkAutoFloor[] Floors { get; set; }
    }

    public class HkAutoFloor
    {
        public int FloorNo { get; set; }
        public HkAutoItem[] Items { get; set; }
    }

    public class HkAutoItem
    {
        public string ItemCode { get; set; }
        public int ItemWidth { get; set; }
        public int ItemHeight { get; set; }
        public int PlacedX { get; set; }
        public int PlacedY { get; set; }
        public bool IsPlaced { get; set; }
        public bool IsRotated { get; set; }
        public int ItemOrder { get; set; }
    }
}
