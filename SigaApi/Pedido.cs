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
    
    public partial class Pedido
    {
        public int Id { get; set; }
        public string NumeroOrden { get; set; }
        public string ClienteId { get; set; }
        public System.DateTime FechaOrden { get; set; }
        public decimal Impuestos { get; set; }
        public decimal TotalAPagar { get; set; }
        public string CreadoPor { get; set; }
        public int Sincronizado { get; set; }
        public int Compagnia { get; set; }
        public string Estado { get; set; }
        public int IsDelete { get; set; }
    }
}
