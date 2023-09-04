using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace PickNPlace.DataAccess
{
    public class PalletRecipeFloorItem
    {
        public int Id { get; set; }

        [ForeignKey("PalletRecipe")]
        public int? PalletRecipeId { get; set; }

        [ForeignKey("PalletRecipeFloor")]
        public int? PalletRecipeFloorId { get; set; }
        public int? ItemOrder { get; set; }
        public int? Row { get; set; }
        public int? Col { get; set; }
        public bool IsVertical { get; set; }

        public PalletRecipe PalletRecipe { get; set; }
        public PalletRecipeFloor PalletRecipeFloor { get; set; }

    }
}
