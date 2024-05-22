using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NTTShopAdmin.Models
{
    public class Pedido
    {
        public int idPedido { get; set; }
        public DateTime fechaPedido { get; set; }
        public int idEstado { get; set; }
        public decimal totalPrecio { get; set; }
        public int idUsuario { get; set; }

        public List<DetallePedido> detallePedido { get; set; }



    }
}