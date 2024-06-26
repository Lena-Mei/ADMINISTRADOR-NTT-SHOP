﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTTShopAdmin.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;

namespace NTTShopAdmin.Controllers
{
    public class PedidoController : Controller
    {
        private string generalUrl = "http://localhost:5000/api/";

        private void Datos()
        {
            var estados = GetAllEstados().ToList();


            var usuarios = ListadoUsuario().ToList();

            ViewBag.Estados = estados;
            ViewBag.Usuarios = usuarios;
        }
        // GET: Pedido
        public ActionResult Index(int? pageSize, int? page, DateTime? desde, DateTime? hasta, int? idEstado )
        {
            if (Session["IdUser"] == null && Session["LoginUser"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            ViewData["SinPedido"] = false;
            string fechaDesde = desde?.ToString("yyyy-MM-dd");
            string fechaHasta = hasta?.ToString("yyyy-MM-dd");

            // Almacena las fechas formateadas en el ViewBag
            ViewBag.Desde = fechaDesde;
            ViewBag.Hasta = fechaHasta;
            ViewBag.Estado = idEstado;
            ViewBag.PageSize = pageSize;

            var pedidos = GetAllPedidos(fechaDesde, fechaHasta, idEstado);
            if (pedidos.Count == 0)
            {
                ViewData["SinPedido"] = true;
            }
            Datos();

            pageSize = (pageSize ?? 5);
            page = (page ?? 1);

            var pedidosOrdenados = pedidos.OrderByDescending(p => p.fechaPedido).ToList();


            return View(pedidosOrdenados.ToPagedList(page.Value, pageSize.Value));
        }

        [HttpGet]
        public ViewResult Detalle(int idPedido, int idUsuario)
        {
            if (TempData["sePuede"] is null)
            {
                TempData["sePuede"] = false;
            }
            Usuario usuario = GetUsuario(idUsuario);
            Pedido pedido = getPedido(idPedido, usuario.IsoIdioma, usuario.idRate);
            Datos();
            return View(pedido);
        }

        [HttpGet]
        public ActionResult ActEstado(int idPedido, int idEstado, int idUsuario)
        {
           
            if (ActualizarEstado(idPedido, idEstado))
            {
                if (idEstado == 1)
                {

                    if(!RestarStock(idPedido, idUsuario))
                    {
                        TempData["sePuede"] = true;
                        ActualizarEstado(idPedido, 5);
                    }
                    else
                    {
                        TempData["sePuede"] = false;
                    }

                }
                if (idEstado == 2) SumarStock(idPedido, idUsuario);

                return RedirectToAction("Detalle", "Pedido", new { idPedido = idPedido, idUsuario=idUsuario });
            }
            else
            {
                return View("Error");
            }
        }
        private bool RestarStock(int idPedido, int idUsuario)
        {
            bool sePuede = true;
            Usuario user = GetUsuario(idUsuario);
            Pedido ped = getPedido(idPedido, user.IsoIdioma, user.idRate);
            Producto prod;
            string error;
            foreach (DetallePedido item in ped.detallePedido)
            {
                var resta =0;
                prod = item.producto;
                resta = prod.stock - item.unidades;
                if (resta < 0)
                {
                    sePuede = false;
                }
                else if(sePuede)
                {
                    prod.stock = resta;
                    ActualizarProducto(prod, out error);
                }
            }
            return sePuede;

        }
        private void SumarStock(int idPedido, int idUsuario)
        {
            Usuario user = GetUsuario(idUsuario);
            Pedido ped = getPedido(idPedido, user.IsoIdioma, user.idRate);
            Producto prod;
            string error;
            foreach (DetallePedido item in ped.detallePedido)
            {
                prod = item.producto;
                prod.stock = prod.stock + item.unidades;
                ActualizarProducto(prod, out error);
            }
        }
        private Pedido getPedido(int idPedido, string idioma, int idRate)
        {
            Pedido pedido = new Pedido();
            try
            {
                string url = generalUrl + "Pedido/getPedido?id="+idPedido+"&idioma=" +idioma+"&idRate="+idRate;
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "GET";

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var resultado = streamReader.ReadToEnd();
                    var json = JObject.Parse(resultado);
                    pedido = json["pedido"].ToObject<Pedido>();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return pedido;
        }
        private bool ActualizarProducto(Producto producto, out string error)
        {
            error = "";
            bool correcto = false;
            var adminData = new { producto = producto };

            string jsonDatos = JsonConvert.SerializeObject(adminData);
            string url = generalUrl + "Producto/updateProducto";
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
        private List<Pedido> GetAllPedidos(string fechaDesde, string fechaHasta, int? idEstado = null)
        {
            List<Pedido> pedidos = new List<Pedido>();
            try
            {
                string url = generalUrl + "Pedido/getAllPedidos?fechaDesde="+ fechaDesde +"&fechaHasta="+ fechaHasta +"&idEstado=" + idEstado;
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "POST";

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

        private bool ActualizarEstado(int idPedido,int idEstado)
        {
            bool correcto = false;
            try
            {
                string url = generalUrl + "Pedido/updateEstadoPedido/" + idPedido + "/" + idEstado;

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
        private Producto GetProducto(int? id)
        {
            Producto producto = new Producto();
            string url = generalUrl + "Producto/getProducto?id=" + id;
            try
            {
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "GET";

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var resultado = streamReader.ReadToEnd();
                    var json = JObject.Parse(resultado);
                    producto = json["producto"].ToObject<Producto>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                producto = null;
            }
            return producto;
        }
    }
}