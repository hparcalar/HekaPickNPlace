using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PickNPlace.DTO
{
    public class PalletStateDTO
    {
        public int Id { get; set; }

        public int? PalletNo { get; set; }
        public bool? IsPickable { get; set; }
        public bool? IsDropable { get; set; }
        public int? PlaceRequestId { get; set; }
        public int? PalletRecipeId { get; set; }

        public int CurrentItemNo { get; set; }
        public bool? CurrentItemIsStarted { get; set; }
        public bool? CurrentItemIsCompleted { get; set; }

        public int CurrentBatchNo { get; set; }
        public bool? CurrentBatchIsStarted { get; set; }
        public bool? CurrentBatchIsCompleted { get; set; }

        public int CompletedBatchCount { get; set; }
    }
}
