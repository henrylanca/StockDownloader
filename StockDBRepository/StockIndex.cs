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
    
    public partial class StockIndex
    {
        public StockIndex()
        {
            this.StockSymbols = new HashSet<StockSymbol>();
        }
    
        public string IndexName { get; set; }
        public string Description { get; set; }
        public string CountryCode { get; set; }
    
        public virtual ICollection<StockSymbol> StockSymbols { get; set; }
    }
}
