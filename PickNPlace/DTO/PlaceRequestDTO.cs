using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PickNPlace.DTO
{
    [Serializable()]
    public class PlaceRequestDTO
    {
        public int Id { get; set; }
        public string RequestNo { get; set; }
        public int BatchCount { get; set; }
        public string RecipeCode { get; set; }
        public string RecipeName { get; set; }

        public PlaceRequestItemDTO[] Items { get; set; }
    }
}
