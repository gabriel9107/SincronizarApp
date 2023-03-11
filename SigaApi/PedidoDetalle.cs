using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigaApi
{
    public partial class PedidoDetalle
    {
         
            public int Id { get; set; }
            public decimal Precio { get; set; }
            public int Cantidad { get; set; }
            public int ProductoId { get; set; }
            public string Codigo { get; set; }
            public string Nombre { get; set; }
            public Nullable<int> PedidoId { get; set; }
            public int Sincronizado { get; set; }
            public int IsDelete { get; set; }
            public int Compagnia { get; set; }
        }
    }

