using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace NTTShopAdmin.Models
{
    public class Producto
    {
        public int idProducto { get; set; }

        [Required(ErrorMessage = "Debes de introducir una cantidad al producto.")]
        [Range(0, int.MaxValue, ErrorMessage = "El campo Stock no puede ser negativo.")]
        public int stock { get; set; }
        public bool habilitado { get; set; }
        public string imagen { get; set; } = "sinImagen";

        public List <DesProducto> descripcion { get; set; }
        public List <ProductoRate> rate { get; set; }

        //[NotMapped]
        //public IFormFile File { get; set; }
    }
}