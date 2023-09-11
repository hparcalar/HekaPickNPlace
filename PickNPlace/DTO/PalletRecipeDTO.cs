using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PickNPlace.DTO
{
    public class PalletRecipeDTO
    {
        public int Id { get; set; }
        public string RecipeCode { get; set; }
        public int PalletWidth { get; set; }
        public int PalletLength { get; set; }
        public int TotalFloors { get; set; }
        public string Explanation { get; set; }

        public PalletRecipeFloorDTO[] Floors { get; set; }
    }
}
