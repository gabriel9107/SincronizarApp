//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SigaApi
{
    using System;
    using System.Collections.Generic;
    
    public partial class InvoiceLines
    {
        public int ID { get; set; }
        public Nullable<int> Qty { get; set; }
        public Nullable<int> ProductID { get; set; }
        public string ProductCode { get; set; }
        public decimal Price { get; set; }
        public Nullable<int> InvoiceID { get; set; }
        public bool IsNew { get; set; }
        public bool IsInAx { get; set; }
        public Nullable<System.DateTime> IntegrationDate { get; set; }
    
        public virtual Invoices Invoices { get; set; }
    }
}