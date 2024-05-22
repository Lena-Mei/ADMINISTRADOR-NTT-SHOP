using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NTTShopAdmin.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace NTTShopAdmin.Controllers
{
    public class ProductoController : Controller
    {
        private string generalUrl = "http://localhost:5000/api/";


        private void DropDown()
        {
            var listaIdioma = GetAllIdioma();
            var listaRates = GetAllRate();
            ViewBag.Rates = listaRates;
            ViewBag.Idiomas = listaIdioma;
        }
        // GET: Producto
        public ViewResult Index(int? pageSize, int? page, string strBusqueda)
        {
           
                List<Producto> listadoProductos = GetAllProducto();

                if (!String.IsNullOrEmpty(strBusqueda))
                {
                    strBusqueda = strBusqueda.ToLower();
                    listadoProductos = listadoProductos
                        .Where(p => p.descripcion[0].nombre.ToLower().Contains(strBusqueda))
                        .ToList();
                }

                pageSize = (pageSize ?? 4);
                page = (page ?? 1);

                ViewBag.PageSize = pageSize;
                ViewBag.SearchString = strBusqueda;

                return View(listadoProductos.ToPagedList(page.Value, pageSize.Value));
            

        }


        [HttpGet]
        public ViewResult Crear()
        {
            DropDown();

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Crear(Producto producto)
        {
            DropDown();
            if (ModelState.IsValid)
            {
                bool correcto = InsertarProducto(producto);

                if (correcto)
                {

                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Mensaje = true;
                }
            }
            return View(producto);
        }


        [HttpGet]
        public ViewResult Editar(int? id)
        {
            var edit = GetProducto(id);
            DropDown();
            var viewModel = new Editar
            {
                producto = edit,
                desProducto = new DesProducto(),
                productoRate = new ProductoRate()
            };
            viewModel.IdProducto();


            return View(viewModel);
        }



        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Editar(Editar viewModel)
        {
            string error = "";
          
            bool rate = true;
            bool des = true;
            viewModel.IdProducto();
            DropDown();
            //Comprobamos que no haya habido un cambio, si es así, se iguala a FALSE.
            if (viewModel.productoRate.idRate == 0 && viewModel.productoRate.precio == 0)
            {
                rate = false; //Si NO se ha añadido nada, se devuelve FALSE
                viewModel.productoRate = null;
            }
            if (viewModel.desProducto.nombre is null && viewModel.desProducto.descripcion is null)
            {
                des = false;
                viewModel.desProducto = null;
            }

            //Si se ha añadido tanto desProducto como ProductoRate:
            if (rate && des)
            {
                if (ModelState.IsValid)
                {
                    viewModel.producto.rate.Add(viewModel.productoRate);
                    viewModel.producto.descripcion.Add(viewModel.desProducto);

                    bool correcto = ActualizarProducto(viewModel.producto, out error);
                    if (correcto)
                    {
                        TempData["correcto"] = true;
                        return RedirectToAction("Editar", viewModel.producto.idProducto);
                    }
                }
                else
                {
                    TempData["correcto"] = false;
                    return View(viewModel);
                }
            }
            else  //Si se ha añadido alguno o no
            {
                var validacion = new ValidationContext(viewModel);
                var validationResult = new List<ValidationResult>();
                if (Validator.TryValidateObject(viewModel, validacion, validationResult, true))
                {
                    if (rate)//Si se ha añadido un rate
                    { 
                        viewModel.producto.rate.Add(viewModel.productoRate);
                    }
                    if (des)
                    {
                        viewModel.producto.descripcion.Add(viewModel.desProducto);
                    }
                    bool correcto = ActualizarProducto(viewModel.producto, out error);
                    if (correcto)
                    {
                        TempData["correcto"] = true;
                        return RedirectToAction("Editar", viewModel.producto.idProducto);
                    }
                }
                else
                {
                    TempData["correcto"] = false;
                    return View(viewModel);
                }

            }
            return View("Error");
        }

        private Producto GetProducto(int? id)
        {
            Producto producto = new Producto();
            string url = generalUrl + "Producto/getProducto?id="+id;
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

        private bool ActualizarPrecio(int idProducto, int idRate, decimal precio)
        {
            bool correcto = false;
            string url = generalUrl + "Producto/setPrecio?idProducto=" + idProducto + "&idRate=" + idRate + "&precio=" + precio;
            try
            {
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "PUT";

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var resultado = streamReader.ReadToEnd();
                    correcto = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return correcto;
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

        private bool InsertarProducto(Producto producto)
        {
            bool correcto = false;
            var productoData = new { producto = producto };
            string url = generalUrl + "Producto/insertProducto";
            string jsonDatos = JsonConvert.SerializeObject(productoData);
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
                throw new Exception(ex.Message, ex);
            }
            return correcto;
        }

        private List<Producto> GetAllProducto()
        {
            List<Producto> productos = new List<Producto>();
            try
            {
                string url = generalUrl + "Producto/getAllProductos?idioma=" + null;
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "GET";

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var resultado = streamReader.ReadToEnd();
                    var json = JObject.Parse(resultado);
                    var productoArray = json["productoLista"].ToObject<JArray>();
                    productos = productoArray.ToObject<List<Producto>>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return productos;
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




    }
}