//------------------------------------------------------------------------------
// <auto-generated>
//    Este código se generó a partir de una plantilla.
//
//    Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//    Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SigaApi
{
    using System;
    using System.Collections.Generic;
    
    public partial class Factura
    {
        public string FacturaId { get; set; }
        public System.DateTime FacturaFecha { get; set; }
        public System.DateTime FacturaVencimiento { get; set; }
        public int Id { get; set; }
        public int IsDelete { get; set; }
        public string MetodoDePago { get; set; }
        public decimal MontoFactura { get; set; }
        public decimal MontoPendiente { get; set; }
        public string PedidoId { get; set; }
        public int Sincronizado { get; set; }
        public string clienteId { get; set; }
        public string clienteNombre { get; set; }
        public int totalPagado { get; set; }
        public string vendedorId { get; set; }
    }
}
