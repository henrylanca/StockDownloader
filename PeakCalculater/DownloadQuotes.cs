using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using StockDownloader.StockDBRepository;

namespace PeakCalculater
{

    public sealed class DownloadQuotes : CodeActivity
    {
        public InArgument<string> MySymbol { get; set; }
        public InArgument<string> MyConnString { get; set; }

        public enum TimeFrame { Day = 1, Week = 2 };
        private enum QuoteOrder { PriceDate = 0, OpenPrice = 1, HighPrice = 2, LowPrice = 3, ClosePrice = 4, Volume = 5, AdjustedClose = 6 };

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override void Execute(CodeActivityContext context)
        {
            string symbol = context.GetValue(this.MySymbol);
            string strConn = this.MyConnString.Get(context);

            try
            {

                using (StockDataEntities dbContext = new StockDataEntities())
                {
                    //dbContext.CommandTimeout = 120;

                    StockSymbol sSymbol = (from s in dbContext.StockSymbols
                                           where s.Symbol == symbol
                                           select s).FirstOrDefault();

                    if (sSymbol == null)
                        throw new ApplicationException("Cannot find Symbol: " + symbol);

                    DateTime startDate = sSymbol.EndDate.HasValue ? sSymbol.EndDate.Value : new DateTime(1990, 1, 1);
                    DateTime endDate = Helper.GetEndDate(DateTime.Now);

                    while (startDate.DayOfWeek != DayOfWeek.Saturday)
                        startDate = startDate.AddDays(-1);

                    while (endDate.DayOfWeek != DayOfWeek.Saturday)
                        endDate = endDate.AddDays(1);

                    if (endDate > startDate)
                    {
                        var delQuotes = from s in dbContext.StockQuotes
                                        where s.Symbol == symbol
                                        && s.QuoteDate >= startDate && s.QuoteDate <= endDate
                                        select s;

                        dbContext.StockQuotes.RemoveRange(delQuotes);
                        dbContext.SaveChanges();


                        //Dowload daily quotes first; then download weekly quotes
                        for (int i = 1; i <= 2; i++)
                        {
                            TimeFrame timeFrame = TimeFrame.Day;

                            if (i == 2)
                                timeFrame = TimeFrame.Week;

                            #region 1. Download Quotes

                            var quotes = Historical.Get(symbol, startDate, endDate, (TimeFrame)(i));
                            foreach (HistoryPrice quote in quotes)
                            {
                                StockQuote stockQuote = new StockQuote();
                                decimal close = (decimal)quote.Close;
                                decimal adjustClose = (decimal)quote.AdjClose;
                                decimal ratio = close == 0 ? 0 : adjustClose / close;

                                stockQuote.Symbol = symbol;
                                stockQuote.QuoteDate = quote.Date;
                                stockQuote.OpenValue = (decimal)quote.Open * ratio;
                                stockQuote.CloseValue = (Decimal)adjustClose;
                                stockQuote.LowValue = (decimal)quote.Low * ratio;
                                stockQuote.HighValue = (decimal)quote.High * ratio;
                                stockQuote.Volume = (long)quote.Volume;
                                stockQuote.TimeFrame = (short)timeFrame;

                                dbContext.StockQuotes.Add(stockQuote);
                            }
                            dbContext.SaveChanges();

                            #endregion

                            #region 2. Calculate Extended Value
                            {
                                DateTime loadStartDate = new DateTime(1990, 1, 1);
                                DateTime startCalculteDate = loadStartDate;

                                StockSymbol symbolItem = (from s in dbContext.StockSymbols
                                                          where s.Symbol == symbol
                                                          select s).SingleOrDefault();

                                if(symbolItem.EndDate.HasValue)
                                {
                                    loadStartDate = symbolItem.EndDate.Value.AddYears(-2);
                                    startCalculteDate = symbolItem.EndDate.Value;
                                }

                                var delExtendQuotes = from s in dbContext.StockQuoteExtents
                                                where s.Symbol == symbol
                                                && s.QuoteDate >= startDate && s.QuoteDate <= endDate
                                                && s.TimeFrame == (short)timeFrame
                                                      select s;

                                dbContext.StockQuoteExtents.RemoveRange(delExtendQuotes);
                                dbContext.SaveChanges();

                                DateTime prevLoadStartDate = loadStartDate.AddDays(-4);
                                List<StockQuote> loadQuotes = dbContext.StockQuotes.Where(q => q.Symbol == symbol && q.TimeFrame == (short)timeFrame && q.QuoteDate >= prevLoadStartDate)
                                    .OrderBy(q => q.QuoteDate).ToList();

                                StockQuoteExtent prevExtQuote = dbContext.StockQuoteExtents.Where(q => q.Symbol == symbol && q.TimeFrame == (short)timeFrame && q.QuoteDate <= loadStartDate)
                                    .OrderByDescending(q => q.QuoteDate).FirstOrDefault();

                                List<StockQuoteExtent> prevQuoteExtents = null;
                                if(prevExtQuote!=null)
                                {
                                    prevQuoteExtents = dbContext.StockQuoteExtents.Where(q => q.Symbol == prevExtQuote.Symbol && q.TimeFrame == (short)timeFrame && q.QuoteDate == prevExtQuote.QuoteDate)
                                    .ToList();
                                }

                                CalculateExtendedValue calculator = new CalculateExtendedValue(loadQuotes, prevQuoteExtents, startCalculteDate, endDate);

                                var extendQuotes = calculator.Execute();

                                dbContext.StockQuoteExtents.AddRange(extendQuotes);
                                dbContext.SaveChanges();

                            }

                            #endregion

                            #region 3. Update Symbol's Start/End Date
                            if (timeFrame == TimeFrame.Week)
                            {
                                StockSymbol symbolItem = (from s in dbContext.StockSymbols
                                                          where s.Symbol == symbol
                                                          select s).SingleOrDefault();

                                DateTime firstQuoteDate = (from q in dbContext.StockQuotes
                                                           where q.Symbol == symbol
                                                           && q.TimeFrame == (short)timeFrame
                                                           orderby q.QuoteDate
                                                           select q.QuoteDate).FirstOrDefault();

                                DateTime lastQuoteDate = (from q in dbContext.StockQuotes
                                                          where q.Symbol == symbol
                                                          && q.TimeFrame == (short)timeFrame
                                                          orderby q.QuoteDate descending
                                                          select q.QuoteDate).FirstOrDefault();

                                if (symbolItem != null)
                                {
                                    try
                                    {
                                        symbolItem.StartDate = firstQuoteDate;
                                        symbolItem.EndDate = lastQuoteDate;

                                        dbContext.SaveChanges();
                                    }
                                    catch (Exception e)
                                    {
                                    }
                                }
                            }

                            dbContext.SaveChanges();
                        }
                        #endregion
                    }

                }
            }
            catch(Exception exp)
            {
                throw new ApplicationException(string.Format("Quote Download Error - {0}", symbol), exp);
            }


        }
    }
}
