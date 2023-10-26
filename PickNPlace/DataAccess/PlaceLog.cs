using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace PickNPlace.DataAccess
{
    public class PlaceLog
    {
        public int Id { get; set; }
        public int PalletNo { get; set; }
        public DateTime PlaceDate { get; set; }
        public bool IsPlaced { get; set; }
        public bool IsDropped { get; set; }
    }
}
