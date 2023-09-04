using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace PickNPlace.DataAccess
{
    public class PalletStateLive
    {
        public int Id { get; set; }

        public int? PalletNo { get; set; }
        public bool? IsPickable { get; set; }
        public bool? IsDropable { get; set; }

        [ForeignKey("PlaceRequest")]
        public int? PlaceRequestId { get; set; }

        [ForeignKey("PalletRecipe")]
        public int? PalletRecipeId { get; set; }

        public int CurrentItemNo { get; set; }
        public bool? CurrentItemIsStarted { get; set; }
        public bool? CurrentItemIsCompleted { get; set; }

        public int CurrentBatchNo { get; set; }
        public bool? CurrentBatchIsStarted { get; set; }
        public bool? CurrentBatchIsCompleted { get; set; }

        public int CompletedBatchCount { get; set; }

        public virtual PlaceRequest PlaceRequest { get; set; }
        public virtual PalletRecipe PalletRecipe { get; set; }
    }
}
