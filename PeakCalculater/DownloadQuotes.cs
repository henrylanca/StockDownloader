using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;

using DBModel;

namespace PeakCalculater
{

    public sealed class DownloadQuotes : CodeActivity
    {
        public InArgument<string> MySymbol { get; set; }
        public InArgument<string> MyConnString { get; set; }

        private enum TimeFrame { Day = 1, Week = 2 };
        private enum QuoteOrder { PriceDate = 0, OpenPrice = 1, HighPrice = 2, LowPrice = 3, ClosePrice = 4, Volume = 5, AdjustedClose = 6 };

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override void Execute(CodeActivityContext context)
        {
            string symbol = context.GetValue(this.MySymbol);
            string strConn = this.MyConnString.Get(context);

            using (StockDBDataContext dbContext = new StockDBDataContext(strConn))
            {
                dbContext.CommandTimeout = 120;

                StockSymbol  sSymbol = (from s in dbContext.StockSymbols
                               where s.Symbol == symbol
                               select s).FirstOrDefault();
 
                if(sSymbol == null)
                    throw new ApplicationException("Cannot find Symbol: " + symbol);

                DateTime startDate = sSymbol.EndDate.HasValue ? sSymbol.EndDate.Value  : new DateTime(1990,1,1);
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

                    dbContext.StockQuotes.DeleteAllOnSubmit(delQuotes);
                    dbContext.SubmitChanges();  


                    //Dowload daily quotes first; then download weekly quotes
                    for (int i = 0; i <= 1; i++)
                    {
                        TimeFrame timeFrame = TimeFrame.Day;

                        if (i == 1)
                            timeFrame = TimeFrame.Week;

                        #region 1. Download Quotes
                        string webAddress = "http://ichart.yahoo.com/table.csv?s=[%s]&a=[%m1]&b=[%d1]&c=[%y1]&d=[%m2]&e=[%d2]&f=[%y2]&g=[%Type]&ignore=.csv";

                        webAddress = webAddress.Replace("[%s]", symbol);
                        webAddress = webAddress.Replace("[%m1]", (startDate.Month - 1).ToString());
                        webAddress = webAddress.Replace("[%d1]", startDate.Day.ToString());
                        webAddress = webAddress.Replace("[%y1]", startDate.Year.ToString());
                        webAddress = webAddress.Replace("[%m2]", (endDate.Month - 1).ToString());
                        webAddress = webAddress.Replace("[%d2]", endDate.Day.ToString());
                        webAddress = webAddress.Replace("[%y2]", endDate.Year.ToString());

                        string dateType = string.Empty;
                        if (timeFrame == TimeFrame.Day)
                            dateType = "d";
                        else if (timeFrame == TimeFrame.Week)
                            dateType = "w";

                        webAddress = webAddress.Replace("[%Type]", dateType);

                        List<string> lstQuote = HttpLib.GetHttpRespsonse(webAddress);

                        if (lstQuote.Count > 0)
                        {
                            foreach (string strQuote in lstQuote)
                            {
                                string[] items = strQuote.Split(',');

                                if (Microsoft.VisualBasic.Information.IsDate(items[0]))
                                {
                                    StockQuote stockQuote = new StockQuote();
                                    decimal close = Convert.ToDecimal(items[(int)QuoteOrder.ClosePrice]);
                                    decimal adjustClose = Convert.ToDecimal(items[(int)QuoteOrder.AdjustedClose]);
                                    decimal ratio = adjustClose / close;

                                    stockQuote.Symbol = symbol;
                                    stockQuote.QuoteDate = Convert.ToDateTime(items[(int)QuoteOrder.PriceDate]);
                                    stockQuote.OpenValue = Convert.ToDecimal(items[(int)QuoteOrder.OpenPrice]) * ratio;
                                    stockQuote.CloseValue = (Decimal)adjustClose;
                                    stockQuote.LowValue = Convert.ToDecimal(items[(int)QuoteOrder.LowPrice]) * ratio;
                                    stockQuote.HighValue = Convert.ToDecimal(items[(int)QuoteOrder.HighPrice]) * ratio;
                                    stockQuote.Volume = Convert.ToInt64 (items[(int)QuoteOrder.Volume]);
                                    stockQuote.TimeFrame = (short)timeFrame;

                                    dbContext.StockQuotes.InsertOnSubmit(stockQuote);                                    
                                }
                            }

                            dbContext.SubmitChanges();
                        }
                        #endregion

                        #region 2. Update Symbol's Start/End Date
                        if (timeFrame == TimeFrame.Day)
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

                                    dbContext.SubmitChanges();
                                }
                                catch (Exception e)
                                {
                                }
                            }
                        }

                        #endregion
                    }
                }

            }

        }
    }
}
