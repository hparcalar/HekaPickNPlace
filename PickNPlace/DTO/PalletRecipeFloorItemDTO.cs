using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PickNPlace.DTO
{
    public class PalletRecipeFloorItemDTO
    {
        public int Id { get; set; }
        public int? PalletRecipeId { get; set; }
        public int? PalletRecipeFloorId { get; set; }
        public int? ItemOrder { get; set; }
        public int? Row { get; set; }
        public int? Col { get; set; }
        public bool IsVertical { get; set; }
    }
}
