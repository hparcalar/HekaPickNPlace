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

        public string ItemCode { get; set; }
    }
}
