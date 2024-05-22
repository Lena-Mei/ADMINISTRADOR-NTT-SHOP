using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NTTShopAdmin.Models
{
    public class Rate
    {
        public int idRate {  get; set; }
        public string descripcion { get; set; }
        public bool defecto { get; set; }
    }
}