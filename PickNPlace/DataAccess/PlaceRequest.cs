using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace PickNPlace.DataAccess
{
    public class PlaceRequest
    {
        public PlaceRequest()
        {
            this.Items = new HashSet<PlaceRequestItem>();
        }

        public int Id { get; set; }
        public string RequestNo { get; set; }
        public int BatchCount { get; set; }
        public string RecipeCode { get; set; }
        public string RecipeName { get; set; }

        [InverseProperty("PlaceRequest")]
        public virtual ICollection<PlaceRequestItem> Items { get; set; }
    }
}
