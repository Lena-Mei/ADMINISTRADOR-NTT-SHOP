using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTTShopAdmin.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace NTTShopAdmin.Controllers
{

    public class UsuarioAdminController : Controller
    {
        // GET: UsuarioAdmin

        //Mejoras 
        private string generalUrl = "http://localhost:5000/api/";

        private void DropDown()
        {
            var listaIdioma = GetAllIdioma();
            ViewBag.Idiomas = listaIdioma;
        }
        public ActionResult Index(int? pageSize, int? page)
        {
            if (Session["IdUser"] != null)
            {
                var listadoAdmin = ListadoAdmin();
                //Si pageSize es nulo, por defecto se pondrá un valor de 10.
                pageSize = (pageSize ?? 10);

                //Al igual que aquí, si el número de página es nulo, se indicará en la página 1
                page = (page ?? 1);

                ViewBag.PageSize = pageSize;
                return View(listadoAdmin.ToPagedList(page.Value, pageSize.Value));
            }
            else
            {
                return View("~/Views/Login/Login.cshtml");
            }
        }

        [HttpGet]
        public ViewResult Crear()
        {
            DropDown();
            if (Session["IdUser"] != null)
            {
                return View();
            }
            else
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult Crear(ManagementUser admin)
        {
            string mensaje = "";
            DropDown();
            if (ModelState.IsValid)
            {
                if (InsertarAdmin(admin, out mensaje))
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Mensaje=mensaje;
                    return View(admin);
                }
            }
            else
            {
                ViewBag.Mnesaje = "Error al añadir un nuevo ADMINISTRADOR";
                return View(admin);
            }
        }

        [HttpGet]
        public ViewResult Delete(int? id)
        {
            if (Session["IdUser"] != null)
            {
                if (id == null || GetAdmin(id) == null)
                {
                    return View("Error");
                }
                else
                {
                    var admin = GetAdmin(id);
                    return View(admin);
                }
            }
            else
            {
                return View("~/Views/Login/Login.cshtml");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (EliminarAdmin(id))
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View("Error");
            }
        }

        [HttpGet]
        public ViewResult Editar(int? id)
        {
            if (Session["IdUser"] != null)
            {

                if (id == null || GetAdmin(id) == null)
                {
                    return View("Error");
                }
                else
                {
                    var edit = GetAdmin(id);
                    return View(edit);
                }
            }
            else
            {
                return View("~/Views/Login/Login.cshtml");
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Editar(ManagementUser objAdmin)
        {
            string error = "";
            if (ModelState.IsValid)
            {
                bool correcto = ActDatosAdmin(objAdmin, out error);
                if (correcto)
                {
                    ViewData["correcto"] = true;
                    return View();

                }
                else
                {
                    ViewData["mensaje"] = error;
                    return View();
                }
            }
            return View(objAdmin);
        }

        [HttpGet]
        public ViewResult CambiarContra (int idAdmin)
        {
            var admin = GetAdmin(idAdmin);
            return View(admin);
        }

        [HttpPost]
        public ActionResult CambiarContra(ManagementUser objAdmin)
        {
            if (ModelState.IsValid)
            {
                if (CambiarContrasenya(objAdmin.idUsuario, objAdmin.contrasenya))
                {
                    ViewBag.Correcto = true;
                }
                else
                {
                    ViewBag.Mensaje = "La contraseña que has introducido es la misma que la anterior.";
                }
            }
            return View(objAdmin);
            

           
        }

        private bool ActDatosAdmin(ManagementUser objAdmin, out string error)
        {
            error = "";
            bool correcto = false;
            var adminData = new { gesUsuario = objAdmin };

            string jsonDatos = JsonConvert.SerializeObject(adminData);
            string url = generalUrl + "GestionUsuario/updateGesUsuario";
            try
            {
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "PUT";
                httpRequest.ContentType = "application/json";
                httpRequest.Accept = "application/json";

                using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                {

                    streamWriter.Write(jsonDatos);
                }

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                HttpStatusCode httpStatus = httpResponse.StatusCode;

                if (httpStatus == HttpStatusCode.OK)
                {
                    correcto = true;
                }

            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("400")) //BadRequest
                {
                    error = "Algún dato está vacío o es nulo.";

                }
                else if (ex.Message.Contains("404")) //NotFound
                {

                    error = "Algún dato introducido ya existe o es inválido.";
                }
            }
            return correcto;
        }

        private bool EliminarAdmin(int id)
        {
            bool correcto = false;
            string url = generalUrl + "GestionUsuario/deleteGesUsuario/" + id;
            try
            {
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "DELETE";
                httpRequest.ContentType = "application/json";
                httpRequest.Accept = "application/json";

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                HttpStatusCode httpStatus = httpResponse.StatusCode;

                if (httpStatus == HttpStatusCode.OK)
                {
                    correcto = true;
                }
                else
                {
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        string errorMensaje = streamReader.ReadToEnd();
                        Console.WriteLine(errorMensaje);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            return correcto;
        }

        private List<ManagementUser> ListadoAdmin()
        {
            List<ManagementUser> listaAdmin = new List<ManagementUser>();
            try
            {
                string url = generalUrl + "GestionUsuario/getAllGesUsuario";
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "GET";

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var resultado = streamReader.ReadToEnd();
                    var json = JObject.Parse(resultado);
                    var pedidoArray = json["gesUsuarioLista"].ToObject<JArray>();
                    listaAdmin = pedidoArray.ToObject<List<ManagementUser>>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return listaAdmin;
        }

        private ManagementUser GetAdmin(int? idAdmin)
        {
            ManagementUser admin = new ManagementUser();
            string url = generalUrl + "GestionUsuario/getGesUsuario/" + idAdmin;
            try
            {


                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "GET";

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var resultado = streamReader.ReadToEnd();
                    var json = JObject.Parse(resultado);
                    admin = json["idGesUser"].ToObject<ManagementUser>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                admin = null;
            }
            return admin;
        }

        public bool InsertarAdmin(ManagementUser admin, out string error)
        {
            error = "";
            bool correcto = false;
            var adminData = new { gesUsuario = admin };
            string url = generalUrl + "GestionUsuario/insertGesUsuario";
            string jsonDatos = JsonConvert.SerializeObject(adminData);
            try
            {
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "POST";
                httpRequest.ContentType = "application/json";
                httpRequest.Accept = "application/json";

                using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                {

                    streamWriter.Write(jsonDatos);
                }

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                HttpStatusCode httpStatus = httpResponse.StatusCode;

                if (httpStatus == HttpStatusCode.OK)
                {
                    correcto = true;
                }
                else
                {
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        string errorMensaje = streamReader.ReadToEnd();
                        Console.WriteLine(errorMensaje);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("400")) //BadRequest
                {
                    error = "Algún dato está vacío o no es válido.";

                }
                else if (ex.Message.Contains("404")) //NotFound
                {

                    error = "El nombre de usuario ya está registrado.";
                }
            }
            return correcto;

        }

        private List<Idioma> GetAllIdioma()
        {
            List<Idioma> idiomas = new List<Idioma>();
            try
            {
                string url = generalUrl + "Idioma/getAllIdiomas";
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "GET";

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var resultado = streamReader.ReadToEnd();
                    var json = JObject.Parse(resultado);
                    var pedidoArray = json["idiomaLista"].ToObject<JArray>();
                    idiomas = pedidoArray.ToObject<List<Idioma>>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return idiomas;
        }

        private bool CambiarContrasenya (int idAdmin, string contrasenya)
        {
            bool correcto = false;
            try
            {
                string url = generalUrl + "GestionUsuario/updateGesContrasenya?idAdmin="+idAdmin+"&contrasenya="+contrasenya;

                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "PUT";
                httpRequest.ContentType = "application/json";
                httpRequest.Accept = "application/json";

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                HttpStatusCode httpStatus = httpResponse.StatusCode;

                if (httpStatus == HttpStatusCode.OK)
                {
                    correcto = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                correcto = false;
            }
            return correcto;
        }
    }
}