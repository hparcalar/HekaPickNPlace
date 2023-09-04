using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace PickNPlace.DataAccess
{
    public class PalletRecipeFloor
    {
        public PalletRecipeFloor()
        {
            this.Items = new HashSet<PalletRecipeFloorItem>();
        }
        public int Id { get; set; }

        [ForeignKey("PalletRecipe")]
        public int? PalletRecipeId { get; set; }

        public int FloorNumber { get; set; }
        public int? Rows { get; set; }
        public int? Cols { get; set; }

        public PalletRecipe PalletRecipe { get; set; }

        [InverseProperty("PalletRecipeFloor")]
        public virtual ICollection<PalletRecipeFloorItem> Items { get; set; }
    }
}
