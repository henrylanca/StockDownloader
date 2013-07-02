//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StockDownloader.StockDBRepository
{
    using System;
    using System.Collections.Generic;
    
    public partial class StockQuote
    {
        public int QuoteID { get; set; }
        public string Symbol { get; set; }
        public System.DateTime QuoteDate { get; set; }
        public decimal OpenValue { get; set; }
        public decimal CloseValue { get; set; }
        public decimal HighValue { get; set; }
        public decimal LowValue { get; set; }
        public long Volume { get; set; }
        public short TimeFrame { get; set; }
        public Nullable<decimal> MAD50 { get; set; }
        public Nullable<decimal> MAD200 { get; set; }
    
        public virtual StockSymbol StockSymbol { get; set; }
    }
}
