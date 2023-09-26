using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PickNPlace.DTO
{
    public class PlaceRequestItemDTO
    {
        public int Id { get; set; }
        public int? PlaceRequestId { get; set; }
        public int? RawMaterialId { get; set; }
        public int PiecesPerBatch { get; set; }
        public int? SackType { get; set; }

        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string SackTypeText
        {
            get
            {
                return SackType == 1 ? "40x60" :
                    SackType == 2 ? "30x50" :
                    SackType == 3 ? "50x70" : "";
            }
        }
    }
}
