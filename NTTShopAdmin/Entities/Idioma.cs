using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NTTShopAdmin.Models
{
    public class Idioma
    {
        public int idIdioma { get; set; }
        
        [Required(ErrorMessage = "Es necesario introducir una descripción de Idioma.")]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "El campo DESCRIPCIÓN no puede contener números.")]
        public string descripcion { get; set; }
       
        [Required(ErrorMessage = "Es necesario introducir una ISO.")]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "El campo ISO no puede contener números.")]
        public string iso { get; set; }

        //public SelectList idiomas { get; set; }
    }
}