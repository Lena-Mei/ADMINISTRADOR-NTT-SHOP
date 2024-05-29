using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using NTTShopAdmin.Models;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using System.Web.UI.WebControls;


namespace NTTShopAdmin.Controllers
{
    public class CerrarSesionController : Controller
    {  
        public ActionResult Cerrarsesion()
        {
            Session["IdUser"] = null;
            Session["LoginUser"] = null;
            return RedirectToAction("Login", "Login");
        }
 
    }
}


    
