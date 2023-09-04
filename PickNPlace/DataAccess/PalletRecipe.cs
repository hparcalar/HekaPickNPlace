using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace PickNPlace.DataAccess
{
    public class PalletRecipe
    {
        public PalletRecipe()
        {
            this.Floors = new HashSet<PalletRecipeFloor>();
            this.Items = new HashSet<PalletRecipeFloorItem>();
        }

        public int Id { get; set; }
        public string RecipeCode { get; set; }
        public int PalletWidth { get; set; }
        public int PalletLength { get; set; }
        public int TotalFloors { get; set; }
        public string Explanation { get; set; }

        [InverseProperty("PalletRecipe")]
        public virtual ICollection<PalletRecipeFloor> Floors { get; set; }

        [InverseProperty("PalletRecipe")]
        public virtual ICollection<PalletRecipeFloorItem> Items { get; set; }
    }
}
