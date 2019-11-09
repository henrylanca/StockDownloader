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
        private List<StockQuoteExtent> _prevExtquotes = null;
        private List<StockQuoteExtent> _quoteExtends = new List<StockQuoteExtent>();

        private DateTime _startDate = DateTime.MinValue;
        private DateTime _endDate = DateTime.MinValue;

        private short[] _indicators = new short[] { 10, 20, 50, 60, 100 };

        public CalculateExtendedValue(List<StockQuote> quotes, List<StockQuoteExtent> prevExtQuotes, DateTime startDate, DateTime endDate)
        {
            this._quotes = quotes;
            this._prevExtquotes = prevExtQuotes == null ? new List<StockQuoteExtent>() : prevExtQuotes;
            this._startDate = startDate;
            this._endDate = endDate;
        }

        public List<StockQuoteExtent> Execute()
        {
            StockQuote prevQuote = null;

            //var processQuotes = this._quotes.Where(q => q.QuoteDate >= this._startDate && q.QuoteDate <= this._endDate).ToList();

            var processQuotes = this._quotes;
            prevQuote = this._quotes[0];
            foreach (var item in processQuotes)
            {
                //Get previous quote, which is used to calculate RSI's AvgGain and AvgLoss
                if (item.QuoteDate < this._startDate)
                {
                    prevQuote = item;
                    continue;
                }
                    

                foreach (short indicator in this._indicators)
                {
                    StockQuoteExtent quoteExtent = new StockQuoteExtent();
                    quoteExtent.Symbol = item.Symbol;
                    quoteExtent.QuoteDate = item.QuoteDate;
                    quoteExtent.TimeFrame = item.TimeFrame;

                    var quoteRange = this._quotes.Where(q => q.QuoteDate <= item.QuoteDate).OrderByDescending(q => q.QuoteDate)
                .Take(indicator).ToList();

                //    var quoteRangeExt = this._quotes.Where(q => q.QuoteDate <= item.QuoteDate).OrderByDescending(q => q.QuoteDate)
                //.Take(indicator).ToList();


                    quoteExtent.Indicator = indicator;
                    quoteExtent.MA = CalculateMA(quoteRange);

                    //Calculate AvgGain, AvgLoss, RSI
                    if(this._prevExtquotes != null)
                    {
                        var prevQuoteExt = this._prevExtquotes.Where(q => q.Indicator == indicator).SingleOrDefault();
                        if(prevQuoteExt!=null && prevQuoteExt.AvgGain.HasValue)
                        {
                            if(item.CloseValue>prevQuote.CloseValue)
                            {
                                quoteExtent.AvgGain = (prevQuoteExt.AvgGain.Value * (indicator - 1) + item.CloseValue - prevQuote.CloseValue) / indicator;
                                quoteExtent.AvgLoss = (prevQuoteExt.AvgLoss.Value * (indicator - 1) + 0) / indicator;
                            }
                            else
                            {
                                quoteExtent.AvgGain = (prevQuoteExt.AvgGain.Value * (indicator - 1) + 0) / indicator;
                                quoteExtent.AvgLoss = (prevQuoteExt.AvgLoss.Value * (indicator - 1) + Math.Abs(item.CloseValue - prevQuote.CloseValue)) / indicator;
                            }


                            if (quoteExtent.AvgLoss == 0)
                                quoteExtent.RSI = 0;
                            else
                                quoteExtent.RSI = 100 - (100 / (1 + (quoteExtent.AvgGain / quoteExtent.AvgLoss))); ;
                        }
                        else
                        {
                            if(quoteRange.Count==indicator)
                            {
                                decimal sumGain = 0;
                                decimal sumLoss = 0;
                                decimal prevClose = quoteRange[0].CloseValue;

                                foreach (var curQuote in quoteRange)
                                {
                                    if (prevClose >= curQuote.CloseValue)
                                        sumGain += prevClose - curQuote.CloseValue;
                                    else
                                        sumLoss += curQuote.CloseValue - prevClose;
                                }
                                quoteExtent.AvgGain = sumGain / indicator;
                                quoteExtent.AvgLoss = sumLoss / indicator;
                            }
                        }
                    }

                    quoteExtent.VolumeWeight = CalculateVolumeWeight(quoteRange, item.Volume);
                    quoteExtent.FromHigh = CalcualteHighRatio(quoteRange, item.CloseValue);
                    quoteExtent.FromLow = CalcualteLowRatio(quoteRange, item.CloseValue);

                    this._quoteExtends.Add(quoteExtent);

                    //var prevExtQuote = this._prevExtquotes.Where(q => q.Indicator == indicator).SingleOrDefault();
                    int quoteIndex = this._prevExtquotes.FindIndex(q => q.Indicator == indicator);
                    if (quoteIndex<0)
                        this._prevExtquotes.Add(quoteExtent);
                    else
                    {
                        this._prevExtquotes[quoteIndex] = quoteExtent;
                    }
                        
                }

                prevQuote = item;
            }

            return this._quoteExtends;
        }

        private decimal CalculateMA(List<StockQuote> quoteRange)
        {
            return quoteRange.Average(q => q.CloseValue);
        }

        //private decimal CalculateRSI(List<StockQuote> quoteRange)
        //{
        //    decimal sumUp = 0;
        //    decimal sumDown = 0;
        //    decimal rsi = 0;

        //    for(int i=1; i< quoteRange.Count; i++)
        //    {
        //        if (quoteRange[i].CloseValue >= quoteRange[i - 1].CloseValue)
        //            sumUp += Math.Abs(quoteRange[i].CloseValue - quoteRange[i - 1].CloseValue);
        //        else
        //            sumDown += Math.Abs(quoteRange[i].CloseValue - quoteRange[i - 1].CloseValue);
        //    }

        //    if(sumDown == 0)
        //    {
        //        rsi = 100;
        //    }
        //    else
        //    {
        //        rsi = 100 - (100 / (1 + sumUp / sumDown));
        //    }
        //    return rsi;
        //}

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
