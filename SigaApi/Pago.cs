using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigaApi
{
    public class Pago
    {
        public string ID { get; set; }
        public string ClienteId { get; set; }
        public string VendorId { get; set; }
        public string MetodoDePago { get; set; }
         

        public long MontoPagado { get; set; }
        public string Banco { get; set; }
        public string FechaPago { get; set; }
        public string FechaDeCheque { get; set; }
        public string NumeroDeCheque { get; set; }
        public long Pendiente { get; set; }
        public long Compagni { get; set; }
        public long Sincronizado { get; set; }
        public long IsDelete { get; set; }
    }
}
