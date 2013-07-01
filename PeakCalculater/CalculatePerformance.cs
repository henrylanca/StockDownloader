using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;

using DBModel;

namespace PeakCalculater
{

    public sealed class CalculatePerformance : CodeActivity
    {
        public InArgument<string> MySymbol { get; set; }
        public InArgument<string> MyConnString { get; set; }

        private enum TimeFrame { Day = 1, Week = 2 };

        protected override void Execute(CodeActivityContext context)
        {
            string symbol = context.GetValue(this.MySymbol);
            string strConn = this.MyConnString.Get(context);

            using (StockDBDataContext dbContext = new StockDBDataContext(strConn))
            {
                TradingGame_StockPerformance tspLatest = (from tsp in dbContext.TradingGame_StockPerformances
                                      where tsp.Symbol == symbol
                                      orderby tsp.StartDate descending
                                      select tsp).FirstOrDefault();

                DateTime dtStartDate = new DateTime(1990, 1, 1);
                DateTime dtEndDate = DateTime.Now.AddMonths(-14);
               

                if (tspLatest != null)
                {
                    dtStartDate = tspLatest.StartDate.AddMonths(1);
                }
                else
                {
                    DateTime? dbStartTime = (from s in dbContext.StockSymbols
                                   where s.Symbol == symbol
                                   select s.StartDate).FirstOrDefault();

                    if (dbStartTime.HasValue == false)
                        dtStartDate = DateTime.Now.AddYears(1);
                }


                while (dtStartDate < dtEndDate)
                {
                    TradingGame_StockPerformance objSP = new TradingGame_StockPerformance();

                    //Get Quote of Strat Date
                    StockQuote quote = (from q in dbContext.StockQuotes
                                 where q.Symbol == symbol && q.TimeFrame == (short)TimeFrame.Day
                                    && q.QuoteDate >= dtStartDate && q.QuoteDate <= dtStartDate.AddDays(14)
                                 orderby q.QuoteDate
                                 select q).FirstOrDefault();

                    if (quote != null)
                    {
                        objSP.Symbol = quote.Symbol;
                        objSP.StartDate = quote.QuoteDate;

                        //Get Performance of 3 Month
                        StockQuote quote_3M = (from q in dbContext.StockQuotes
                                               where q.Symbol == symbol && q.TimeFrame == (short)TimeFrame.Day
                                                  && q.QuoteDate >= dtStartDate.AddMonths(3) && q.QuoteDate <= dtStartDate.AddMonths(3).AddDays(14)
                                               orderby q.QuoteDate
                                               select q).FirstOrDefault();

                        if (quote_3M != null)
                        {
                            objSP.ThreeMonth = (double)(quote_3M.CloseValue / quote.CloseValue);
                        }

                        //Get Performance of 6 Month
                        StockQuote quote_6M = (from q in dbContext.StockQuotes
                                               where q.Symbol == symbol && q.TimeFrame == (short)TimeFrame.Day
                                                  && q.QuoteDate >= dtStartDate.AddMonths(6) && q.QuoteDate <= dtStartDate.AddMonths(6).AddDays(14)
                                               orderby q.QuoteDate
                                               select q).FirstOrDefault();

                        if (quote_6M != null)
                        {
                            objSP.SixMonth = (double)(quote_6M.CloseValue / quote.CloseValue);
                        }

                        //Get Performance of 12 Month
                        StockQuote quote_12M = (from q in dbContext.StockQuotes
                                                where q.Symbol == symbol && q.TimeFrame == (short)TimeFrame.Day
                                                   && q.QuoteDate >= dtStartDate.AddMonths(12) && q.QuoteDate <= dtStartDate.AddMonths(12).AddDays(14)
                                                orderby q.QuoteDate
                                                select q).FirstOrDefault();

                        if (quote_12M != null)
                        {
                            objSP.TwelveMonth = (double)(quote_12M.CloseValue / quote.CloseValue);
                        }


                        //Insert Performance Date
                        if (objSP.Symbol.Length > 0)
                        {
                            dbContext.TradingGame_StockPerformances.InsertOnSubmit(objSP);
                            dbContext.SubmitChanges();
                        }
                    }



                    if (dtStartDate.Month == 12)
                    {
                        dtStartDate = new DateTime(dtStartDate.Year + 1, 1, 1);
                    }
                    else
                    {
                        dtStartDate = new DateTime(dtStartDate.Year, dtStartDate.Month + 1, 1);
                    }
                }





            }
        }
    }
}
