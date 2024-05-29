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
    public class LoginController : Controller
    {

        private string generalUrl = "http://localhost:5000/api/";

     
        public ActionResult Login()
        {

            Session["IdUser"] = null;
            Session["LoginUser"] = null;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(ManagementUser objUser)
        {
            string errorMensaje = "";
            ManagementUser usuario = new ManagementUser();
                bool correcto = InicioSesion(objUser.inicio, objUser.contrasenya, out errorMensaje);
                if (correcto)
                {                    
                         return View("~/Views/Home/Index.cshtml");
                }
                else
                {
                    ViewData["errorMensaje"] = errorMensaje;
                    return View();
                }
        }

        private bool InicioSesion(string login, string contrasenya, out string errorMensaje)
        {
            errorMensaje = "";
            bool correcto = false;
            var url = generalUrl + "GestionUsuario/getLoginManagment?inicio=" + login + "&contrasenya=" + contrasenya;
            try
            {
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "GET";

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var resultado = streamReader.ReadToEnd();
                    var json = JObject.Parse(resultado);

                    string id = json["idGesUser"].ToString();

                    Session["IdUser"] = id;
                    Session["LoginUser"] = login;

                    return true;
                }
               
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("404"))
                {
                    errorMensaje = "Usuario o contraseña no válidos.";

                }
                else if (ex.Message.Contains("400"))
                {

                    errorMensaje = "Todos los campos son obligatorios.";
                }
            }

            return correcto;
        }
        public ActionResult UserDashBoard()
        {
            if (Session["UserID"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
    }
}


    
