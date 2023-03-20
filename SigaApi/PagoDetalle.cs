using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigaApi
{
    public class PagoDetalle
    { 
        public int Compagnia { get; set; }  
        public int IsDelete { get; set; }   
        public string facturaId { get; set; }   
        public double montoAplicado { get; set; }
        public double 
montoDeFacturaAlMomento
        { get; set; }
        public  int pagoId { get; set; }
        public int sincronizado { get; set; }
    }
}
