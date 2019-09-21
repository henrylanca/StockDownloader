using StockDownloader.StockDBRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeakCalculater
{
    public class CalculateExtendedValue
    {
        private List<StockQuote> _quotes = null;
        private List<StockQuoteExtent> _quoteExtends = new List<StockQuoteExtent>();

        private DateTime _startDate = DateTime.MinValue;
        private DateTime _endDate = DateTime.MinValue;

        private short[] _indicators = new short[] { 10, 20, 50, 60, 100 };

        public CalculateExtendedValue(List<StockQuote> quotes, DateTime startDate, DateTime endDate)
        {
            this._quotes = quotes;
            this._startDate = startDate;
            this._endDate = endDate;
        }

        public List<StockQuoteExtent> Execute()
        {
            var processQuotes = this._quotes.Where(q => q.QuoteDate >= this._startDate && q.QuoteDate <= this._endDate).ToList();

            foreach(var item in processQuotes)
            {


                foreach(short indicator in this._indicators)
                {
                    StockQuoteExtent quoteExtent = new StockQuoteExtent();
                    quoteExtent.Symbol = item.Symbol;
                    quoteExtent.QuoteDate = item.QuoteDate;
                    quoteExtent.TimeFrame = item.TimeFrame;

                    var quoteRange = this._quotes.Where(q => q.QuoteDate <= item.QuoteDate).OrderByDescending(q => q.QuoteDate)
                .Take(indicator).ToList();

                    quoteExtent.Indicator = indicator;
                    quoteExtent.MA = CalculateMA(quoteRange);
                    quoteExtent.VolumeWeight = CalculateVolumeWeight(quoteRange, item.Volume);
                    quoteExtent.FromHigh = CalcualteHighRatio(quoteRange, item.CloseValue);
                    quoteExtent.FromLow = CalcualteLowRatio(quoteRange, item.CloseValue);

                    this._quoteExtends.Add(quoteExtent);
                }
            
            }

            return this._quoteExtends;
        }

        private decimal CalculateMA(List<StockQuote> quoteRange)
        {
            return quoteRange.Average(q => q.CloseValue);
        }

        private decimal CalculateVolumeWeight(List<StockQuote> quoteRange, decimal volume)
        {
            try
            {
                return volume / (decimal)quoteRange.Average(q => q.Volume);
            }
            catch
            {
                return 0;
            }

        }

        private decimal CalcualteHighRatio(List<StockQuote> quoteRange, decimal close)
        {
            try
            {
                return close / quoteRange.Max(q => q.CloseValue);
            }
            catch
            {
                return 0;
            }
            
        }

        private decimal CalcualteLowRatio(List<StockQuote> quoteRange, decimal close)
        {
            try
            {
                return close / quoteRange.Min(q => q.CloseValue);
            }
            catch
            {
                return 0;
            }
            
        }
    }
}
