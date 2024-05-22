using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NTTShopAdmin.Models
{
    public class ManagementUser
    {
        public int idUsuario { get; set; }
        [Required(ErrorMessage = "Es necesario introducir un Nombre de Usuario.")]

        public string inicio { get; set; }
        [Required(ErrorMessage = "Es necesario introducir una contraseña.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).{10,}$", ErrorMessage = "La contraseña debe de contener mínimo 10 caracteres, una mayúscula y un nñumero. ")]

        public string contrasenya { get; set; }
        [Required(ErrorMessage = "Es necesario introducir un Nombre.")]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "El campo Nombre no puede contener números.")]
        public string nombre { get; set; }
        
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "El campo primer apellido no puede contener números.")]
        [Required(ErrorMessage = "Es necesario introducir un Apellido.")]
        public string apellido1 { get; set; }
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "El campo apelldio no puede contener números.")]

        public string apellido2 { get; set; }
        [Required(ErrorMessage = "Es necesario introducir un correo electrónico.")]
        [Display(Name = "Correo electrónico")]
        [RegularExpression(@"^[^\s@]+@[^\s@]+\.(com|es)$", ErrorMessage = "Dirección de correo inválido. Debe tener '@' y terminar en '.com' o '.es'.")]
        public string email { get; set; }
        public string isoIdioma { get; set; }
    }
}