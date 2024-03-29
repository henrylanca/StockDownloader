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
    
    public partial class StockSymbol
    {
        public StockSymbol()
        {
            this.StockPeaks = new HashSet<StockPeak>();
            this.StockPicks = new HashSet<StockPick>();
            this.StockQuotes = new HashSet<StockQuote>();
            this.StockIndexes = new HashSet<StockIndex>();
        }
    
        public string Symbol { get; set; }
        public string StockName { get; set; }
        public string Sector { get; set; }
        public Nullable<int> ETF { get; set; }
        public Nullable<bool> HasFuture { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public string Country { get; set; }
    
        public virtual StockInformation StockInformation { get; set; }
        public virtual ICollection<StockPeak> StockPeaks { get; set; }
        public virtual ICollection<StockPick> StockPicks { get; set; }
        public virtual ICollection<StockQuote> StockQuotes { get; set; }
        public virtual ICollection<StockIndex> StockIndexes { get; set; }
        public virtual StockCountry StockCountry { get; set; }
    }
}
