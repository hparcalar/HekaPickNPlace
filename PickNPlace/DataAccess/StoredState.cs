using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace PickNPlace.DataAccess
{
    public class StoredState
    {
        public int Id { get; set; }

        public int PalletNo { get; set; }
        public string FullContent { get; set; }
    }
}
