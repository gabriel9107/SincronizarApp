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
    
    public partial class Customer
    {
        public Customer()
        {
            this.SalesOrders = new HashSet<SalesOrder>();
        }
    
        public int ID { get; set; }
        public string City { get; set; }
        public string Clase { get; set; }
        public string Comment1 { get; set; }
        public string Comment2 { get; set; }
        public string Country { get; set; }
        public Nullable<System.DateTime> CreateAt { get; set; }
        public string CreateBy { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerDir { get; set; }
        public string CustomerName { get; set; }
        public string Fax { get; set; }
        public bool IsDelete { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Phone3 { get; set; }
        public string ShippingMth { get; set; }
        public string Street { get; set; }
        public string VendorCode { get; set; }
    
        public virtual ICollection<SalesOrder> SalesOrders { get; set; }
    }
}
