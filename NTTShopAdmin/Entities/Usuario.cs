using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NTTShopAdmin.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Inicio { get; set; }
        public string Contrasenya { get; set; }
        public string Nombre { get; set; }
        public string Apellido1 { get; set; }
        public string Apellido2 { get; set; }
        public string Direccion {  get; set; }
        public string Provincia { get; set; }
        public string Ciudad {  get; set; }
        public string CodigoPostal { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string IsoIdioma { get; set; }
        public int idRate { get; set; }

    }
}