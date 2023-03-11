
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigaApi
{
    public partial class Pedido
    {

        public int id { get; set; }
        public String  ClienteId { get; set; }
        public string NumeroOrden { get; set; }
        public Nullable<System.DateTime> FechaOrden { get; set; }
        public decimal totalAPagar { get; set; }
        public decimal Impuestos { get; set; }
        public string CreadoPor { get; set; }
        public int Sincronizado { get; set; }
        public int IsDelete { get; set; }
        public int Compagnia { get; set; }

    }
    public partial class Pedido2
    {

        public int id { get; set; }
        public String ClienteId { get; set; }
        public string NumeroOrden { get; set; }
        public Nullable<System.DateTime> FechaOrden { get; set; }
        public decimal totalAPagar { get; set; }
        public decimal Impuestos { get; set; }
        public string CreadoPor { get; set; }
        public int Sincronizado { get; set; }
        public int IsDelete { get; set; }
        public int Compagnia { get; set; }

        public string key { get; set; }

    }
}
