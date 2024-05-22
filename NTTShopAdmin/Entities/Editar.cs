using NTTShopAdmin.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NTTShopAdmin.Models
{
    public class Editar :IValidatableObject
    {
        public Producto producto { get; set; }
        public DesProducto desProducto { get; set; }
        public ProductoRate productoRate { get; set; }

        public void IdProducto()
        {
            productoRate.idProducto = producto.idProducto;
            desProducto.idProducto = producto.idProducto;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var result= new List<ValidationResult>();
            if(desProducto != null)
            {
               
                    if (string.IsNullOrEmpty(desProducto.nombre))
                    {
                        result.Add(new ValidationResult("Es necesario introducir un nombre de producto.", new[] { nameof(desProducto.nombre) }));
                    }
                    if (string.IsNullOrEmpty(desProducto.descripcion))
                    {
                        result.Add(new ValidationResult("Es necesario introducir una descripción del producto.", new[] { nameof(desProducto.descripcion) }));
                    }
            }
            if(productoRate != null)
            {
                if (productoRate.idRate == 0) 
                {
                        result.Add(new ValidationResult("Es necesario escoger un tipo de tarifa.", new[] { nameof(productoRate.idRate) }));
                }
                if (productoRate.precio == 0 || productoRate.precio < 0)
                {
                        result.Add(new ValidationResult("Precio inválido.", new[] { nameof(productoRate.precio) }));
                }
            }
            if(producto != null)
            {
                if (string.IsNullOrEmpty(producto.descripcion[0].nombre))
                {
                    result.Add(new ValidationResult("Es necesario introducir un nombre de producto.", new[] { nameof(desProducto.nombre) }));
                }
                if (string.IsNullOrEmpty(producto.descripcion[0].descripcion))
                {
                    result.Add(new ValidationResult("Es necesario introducir una descripción del producto.", new[] { nameof(desProducto.descripcion) }));
                }
                if (producto.rate[0].precio == 0 || producto.rate[0].precio < 0)
                {
                    result.Add(new ValidationResult("Precio inválido.", new[] { nameof(productoRate.precio) }));
                }
            }
            return result;
        }
    }
}