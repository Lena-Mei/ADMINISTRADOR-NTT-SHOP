using System;
using System.Collections.Generic;
using System.Linq;
using NTTShopAdmin.Models;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Web.UI.WebControls;
using PagedList;
using Newtonsoft.Json;

namespace NTTShopAdmin.Controllers
{
    public class UsuarioController : Controller
    {
        private string generalUrl = "http://localhost:5000/api/";
        // GET: Usuario

        private void Datos(int idUsuario)
        {
            var estados = GetAllEstados().ToList();
            ViewBag.Estados = estados;
            var pedidos = GetPedidosUsuario(idUsuario);
            ViewBag.Pedidos = pedidos;
            var rate = GetAllRate().ToList();
            ViewBag.Rate = rate;

        }
        public ViewResult Index(int? pageSize, int? page, string strBusqueda)
        {
            if (Session["IdUser"] != null)
            {
                var listadoUsuario = ListadoUsuario();


                if (!String.IsNullOrEmpty(strBusqueda))
                {
                    strBusqueda = strBusqueda.ToLower();
                    listadoUsuario = listadoUsuario
                        .Where(u=> u.Nombre.ToLower().Contains(strBusqueda))
                        .ToList();
                }

                //Si pageSize es nulo, por defecto se pondrá un valor de 10.
                pageSize = (pageSize ?? 5);

                //Al igual que aquí, si el número de página es nulo, se indicará en la página 1
                page = (page ?? 1);

                ViewBag.PageSize = pageSize;
                ViewBag.SearchString = strBusqueda;

                return View(listadoUsuario.ToPagedList(page.Value, pageSize.Value));
            }
            else
            {
                return View("~/Views/Login/Login.cshtml");
            }
           

        }

        [HttpGet]
        public ViewResult Detalles(int idUsuario)
        {
            Datos(idUsuario);
            var usuario = GetUsuario(idUsuario);
            return View(usuario);
        }

        [HttpPost]
        public ActionResult Actualizar (Usuario usuario)
        {
            bool correcto = ActualizarUsuario(usuario);
            Usuario act = GetUsuario(usuario.IdUsuario);
            if (correcto)
            {
                return RedirectToAction("Detalles", act);
            }
            else
            {
                return View("Error");
            }
        }

        public ActionResult Delete(int idUsuario)
        {
            if (EliminarUsuario(idUsuario))
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return (View("Error"));
            }
        }
        private bool EliminarUsuario(int id)
        {
            bool correcto = false;
            string url = generalUrl + "Usuario/deleteUsuario/" + id;
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
        private Usuario GetUsuario(int? id)
        {
            Usuario usuario = new Usuario();
            string url = generalUrl + "Usuario/getUsuario/" + id;
            try
            {
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "GET";

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var resultado = streamReader.ReadToEnd();
                    var json = JObject.Parse(resultado);
                    usuario = json["idUsuario"].ToObject<Usuario>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                usuario = null;
            }
            return usuario;
        }
        private List<Pedido> GetPedidosUsuario(int ? id)
        {
            List<Pedido> pedidos = new List<Pedido>();
            
            try
            {
                var url = generalUrl + "Pedido/getPedidoIdUser/" + id;
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "GET";

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var resultado = streamReader.ReadToEnd();
                    var json = JObject.Parse(resultado);
                    var pedidoArray = json["pedidoLista"].ToObject<JArray>();
                    pedidos = pedidoArray.ToObject<List<Pedido>>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            return pedidos;

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

        private List<Estado> GetAllEstados()
        {
            List<Estado> estados = new List<Estado>();
            try
            {
                string url = generalUrl + "Pedido/getAllEstados";
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "PUT";

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var resultado = streamReader.ReadToEnd();
                    var json = JObject.Parse(resultado);
                    var estadoArray = json["estadoLista"].ToObject<JArray>();
                    estados = estadoArray.ToObject<List<Estado>>();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return estados;
        }
        private List<Rate> GetAllRate()
        {
            List<Rate> rates = new List<Rate>();
            try
            {
                string url = generalUrl + "Rate/getAllRate";
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "GET";

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var resultado = streamReader.ReadToEnd();
                    var json = JObject.Parse(resultado);
                    var pedidoArray = json["rateLista"].ToObject<JArray>();
                    rates = pedidoArray.ToObject<List<Rate>>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return rates;
        }

        private bool ActualizarUsuario(Usuario usuario)
        {
            bool correcto = false;
            var adminData = new { usuario = usuario };

            string jsonDatos = JsonConvert.SerializeObject(adminData);
            string url = generalUrl + "Usuario/updateUsuario";
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
                throw new Exception(ex.Message, ex);
            }
            return correcto;
        }
    }
}