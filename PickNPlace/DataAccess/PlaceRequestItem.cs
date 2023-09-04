using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace PickNPlace.DataAccess
{
    public class PlaceRequestItem
    {
        public int Id { get; set; }

        [ForeignKey("PlaceRequest")]
        public int? PlaceRequestId { get; set; }

        [ForeignKey("RawMaterial")]
        public int? RawMaterialId { get; set; }

        public int PiecesPerBatch { get; set; }

        public virtual PlaceRequest PlaceRequest { get; set; }
        public virtual RawMaterial RawMaterial { get; set; }
    }
}
