using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NTTShopAdmin.Models
{
    public class DetallePedido
    {
        public int idPedido { get; set; }
        public int idProducto { get; set; }
        public decimal precio { get; set; }
        public int unidades { get; set; }

        public Producto producto { get; set; }
    }
}