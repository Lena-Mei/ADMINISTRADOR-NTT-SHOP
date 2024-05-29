using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTTShopAdmin.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace NTTShopAdmin.Controllers
{
    public class IdiomasController : Controller
    {

        private string generalUrl = "http://localhost:5000/api/";

        // GET: Idiomas
        public ActionResult Index(int? pageSize, int? page)
        {
            if (Session["IdUser"] == null && Session["LoginUser"] == null)
            {
                return RedirectToAction("Login", "Login");
            }



            var listadoIdiomas = getAllIdiomas();
            pageSize = (pageSize ?? 5);

            //Al igual que aquí, si el número de página es nulo, se indicará en la página 1
            page = (page ?? 1);

            ViewBag.PageSize = pageSize;

            return View(listadoIdiomas.ToPagedList(page.Value, pageSize.Value));
        }

        [HttpGet]
        public ActionResult Editar(int? idIdioma)
        {
            if (Session["IdUser"] == null && Session["LoginUser"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (idIdioma == null)
            {
                return View("Error");
            }
            else
            {
                var idioma = getIdioma(idIdioma);
                return View(idioma);
            }
            

        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Editar(Idioma idioma)
        {
            if (Session["IdUser"] == null && Session["LoginUser"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            string error = "";
            if (ModelState.IsValid)
            {
                bool correcto = ActIdioma(idioma, out error);
                if (correcto)
                {
                    ViewData["correcto"] = true;
                    return View();
                }
                else
                {
                    ViewData["error"] = error;
                    return View();
                }
            }
            return View(idioma);
        }

        [HttpGet]
        public ActionResult Crear()
        {
            if (Session["IdUser"] == null && Session["LoginUser"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Crear (Idioma idioma)
        {
            if (Session["IdUser"] == null && Session["LoginUser"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            string mensaje = "";
            if (ModelState.IsValid)
            {
                if (InsertarIdioma(idioma, out mensaje))
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewData["error"] = mensaje;
                    return View(idioma);
                }
            }
            else
            {
                return View(idioma);
            }
        }

        [HttpGet]
        public ViewResult Detalles(int ? idIdioma)
        {
            var idioma = getIdioma(idIdioma);
            var pedidos = ListaProductoIdioma(idioma.iso);
            var usuarios = ListadoUsuario();
            var admins = ListadoAdmin();
            
            List<Usuario> usuariosIdioma = new List<Usuario>();
            foreach (var usuario in usuarios)
            {
                if(usuario.IsoIdioma == idioma.iso)
                {
                    usuariosIdioma.Add(usuario);
                }
            }

            List<ManagementUser> lista = new List<ManagementUser>();
            foreach(var admin in admins)
            {
                if(admin.isoIdioma == idioma.iso)
                {
                    lista.Add(admin);
                }
            }

            if(usuariosIdioma.Count>0 || pedidos.Count > 0 || lista.Count>0)
            {
                ViewBag.Eliminar = false;
            }
            else
            {
                ViewBag.Eliminar = true;
            }
            
            ViewBag.UsuariosIdioma = usuariosIdioma;
            return View(idioma);
        }

        [HttpGet]
        public ActionResult Eliminar(int idIdioma)
        {
            if (Session["IdUser"] == null && Session["LoginUser"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (EliminarIdioma(idIdioma))
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View("Error");
            }
        }

        private bool InsertarIdioma (Idioma idioma, out string error)
        {
            error = "";
            bool correcto = false;
            var idiomaData = new { idioma = idioma };
            string url = generalUrl + "Idioma/insertIdioma";
            string jsonDatos = JsonConvert.SerializeObject(idiomaData);
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
                    error = "Algún dato introducido es inválido.";

                }
                else if (ex.Message.Contains("404")) //NotFound
                {

                    error = "Algún dato introducido ya existe.";
                }
            }
            return correcto;

        }

        private bool ActIdioma (Idioma idioma, out string error)
        {
            
                error = "";
                bool correcto = false;
                var idiomaData = new { idioma = idioma };

                string jsonDatos = JsonConvert.SerializeObject(idiomaData);
                string url = generalUrl + "Idioma/updateIdioma";
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
                    if (ex.Message.Contains("404")) //BadRequest
                    {
                        error = "Algún dato no es válido o está repetido.";

                    }
                    else if (ex.Message.Contains("400")) //NotFound
                    {

                        error = "Algún dato está vacío o es nulo.";
                    }
                else
                {
                    error = "ERROR al ejecutar el procedimiento en la API.";
                }
                }
                return correcto;
            
        }

        private List<Idioma> getAllIdiomas()
        {
            List<Idioma> listaIdiomas = new List<Idioma>();
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
                    listaIdiomas = pedidoArray.ToObject<List<Idioma>>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return listaIdiomas;
        }

        private Idioma getIdioma(int? idIdioma)
        {
            Idioma idioma = new Idioma();
            string url = generalUrl + "Idioma/getIdioma/" + idIdioma;
            try
            {
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "GET";

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var resultado = streamReader.ReadToEnd();
                    var json = JObject.Parse(resultado);
                    idioma = json["idIdioma"].ToObject<Idioma>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                idioma = null;
            }
            return idioma;
        }

        private bool EliminarIdioma (int idIdioma)
        {
            bool correcto = false;
            string url = generalUrl + "Idioma/deleteIdioma/" + idIdioma;
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
    
        private List<Usuario> ListadoUsuario()
        {
            List<Usuario> listaUsuarios = new List<Usuario>();
            try
            {
                string url = generalUrl + "Usuario/getAllUsuario";
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "GET";

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var resultado = streamReader.ReadToEnd();
                    var json = JObject.Parse(resultado);
                    var pedidoArray = json["usuarioLista"].ToObject<JArray>();
                    listaUsuarios = pedidoArray.ToObject<List<Usuario>>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return listaUsuarios;
        }

        private List<Producto> ListaProductoIdioma(string iso)
        {
            List<Producto> listaProductoIdioma = new List<Producto>();
            try
            {
                string url = generalUrl + "Producto/getAllProductos?idioma=" + iso;
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "GET";

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var resultado = streamReader.ReadToEnd();
                    var json = JObject.Parse(resultado);
                    var productoArray = json["productoLista"].ToObject<JArray>();
                    listaProductoIdioma = productoArray.ToObject<List<Producto>>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return listaProductoIdioma;
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

    }
}