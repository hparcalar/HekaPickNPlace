using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PickNPlace.DTO
{
    [Serializable()]
    public class StoredStateDTO
    {
        public HkAutoPallet PalletData { get; set; }
        public PlaceRequestDTO RequestData { get; set; }
        public string ItemCode { get; set; }
        public int ItemPerQuantity { get; set; }
    }
}
