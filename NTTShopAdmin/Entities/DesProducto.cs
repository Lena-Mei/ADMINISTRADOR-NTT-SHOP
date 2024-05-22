using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NTTShopAdmin.Models
{
    public class DesProducto
    {
        public int idDesProducto { get; set; }
        public int idProducto { get; set; }
        public string isoIdioma { get; set; }
        [Display(Name = "Nombre del Producto")]
        [Required(ErrorMessage = "Se requiere introducir el nombre del producto.")]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "El campo Nombre no puede contener números.")]
        public string nombre { get; set; }
        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "Se requiere una descripción del producto.")]
        public string descripcion { get; set; }
    }
}