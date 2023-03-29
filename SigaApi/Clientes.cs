using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigaApi
{
    class Clientes
    {
        
        public int ID { get; set; }

        
        public int activo { get; set; }

        
        public string codigo { get; set; }

        
        public string codigoVendedor { get; set; }

        
        public string comentario { get; set; }

        
        public int compagnia { get; set; }

        public string direccion
        { get; set; }

        public string descuento { get; set; }

        public string nombre { get; set; }

        
        public int sincronizado { get; set; }

        
        public string telefono1 { get; set; }
        public string telefono2 { get; set; }

        public string idFire { get; set; }
    }
}
