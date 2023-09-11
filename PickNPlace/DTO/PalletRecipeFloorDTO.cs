using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PickNPlace.DTO
{
    public class PalletRecipeFloorDTO
    {
        public int Id { get; set; }
        public int? PalletRecipeId { get; set; }

        public int FloorNumber { get; set; }
        public int? Rows { get; set; }
        public int? Cols { get; set; }

        public PalletRecipeFloorItemDTO[] Items { get; set; }
    }
}
