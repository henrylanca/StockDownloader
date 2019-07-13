using StockDownloader.StockDBRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeakCalculater
{
    public class ExtendCalculator
    {
        private List<StockQuote> _quotes;

        public ExtendCalculator(List<StockQuote> quotes)
        {
            this._quotes = quotes;
        }


        //public List<Stock>

        private decimal CalculateMA(decimal[] values)
        {
            return values.Average();
        }
    }
}
