using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NTTShopAdmin.Models
{
    public class ProductoRate
    {
        public int idProducto { get; set; }

        public int idRate { get; set; }

        [RegularExpression(@"^[-0123456789]+[0-9.,]*$",ErrorMessage = "El valor introducido debe ser de tipo monetario.")]
        [Required(ErrorMessage = "Debes de introducir un precio al producto.")]
        [Range(0, double.MaxValue, ErrorMessage = "El campo Precio no puede ser negativo.")]
        public decimal precio { get; set; }


        
    }
}