//------------------------------------------------------------------------------
// <auto-generated>
//    Este código se generó a partir de una plantilla.
//
//    Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//    Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SigaApi.Properties
{
    using System;
    using System.Collections.Generic;
    
    public partial class FaturaDetalle
    {
        public int Id { get; set; }
        public string FacturaNumero { get; set; }
        public string LineaNumero { get; set; }
        public string Nombre { get; set; }
        public Nullable<decimal> Precio { get; set; }
        public string ProductoCodigo { get; set; }
        public Nullable<int> Qty { get; set; }
        public Nullable<decimal> MontoLinea { get; set; }
        public string Sincronizado { get; set; }
    }
}
