using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace PickNPlace.DataAccess
{
    public class SysParam
    {
        public int Id { get; set; }
        public string ParamCode { get; set; }
        public string ParamValue { get; set; }
        public string Explanation { get; set; }
    }
}
