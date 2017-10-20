using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;

using DBModel;

namespace PeakCalculater
{

    public sealed class FallEnough : CodeActivity
    {
        public InArgument<string> MySymbol { get; set; }
        public InArgument<string> MyConnString { get; set; }

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override void Execute(CodeActivityContext context)
        {
            string strSymbol = this.MySymbol.Get(context);
            string strConn = this.MyConnString.Get(context);

            using (StockDBDataContext dbContext = new StockDBDataContext(strConn))
            {
                for(short pickType=3; pickType<=4; pickType++)
                {
                    StockPick lastPick = (from a in dbContext.StockPicks
                                          where a.Symbol == strSymbol
                                          && a.PickType == pickType
                                          orderby a.PickDate descending
                                          select a).FirstOrDefault();

                    DateTime lastDtPick = DateTime.Now.AddYears(-20);

                    if (lastPick != null)
                    {
                        lastDtPick = lastPick.PickDate;
                    }

                    List<StockQuote> lstQuote = (from a in dbContext.StockQuotes
                                                 where a.Symbol == strSymbol
                                                 && a.TimeFrame == (pickType==3 ? 2 : 1)
                                                 && a.QuoteDate >= lastDtPick
                                                 orderby a.QuoteDate
                                                 select a).ToList();

                    int fallWeeks = 0;
                    StockQuote prevQuote = null;
                    int pastWeeks = 0;
                    bool startChecking = false;

                    foreach (StockQuote quote in lstQuote)
                    {
                        if (quote.CloseValue < quote.OpenValue)
                            fallWeeks++;
                        else
                            fallWeeks = 0;

                        if (fallWeeks >= 4)
                        {
                            pastWeeks = 0;
                            startChecking = true;
                        }

                        if (pastWeeks <= 8 && startChecking)
                        {
                            pastWeeks++;
                        }
                        else
                        {
                            startChecking = false;
                        }

                        if (startChecking)
                        {
                            if (quote.CloseValue > quote.OpenValue)
                            {
                                string pickKey = string.Format("{0}_{1:yyyy-MM-dd}_{2}", quote.Symbol,
                                    quote.QuoteDate, pickType);

                                if (quote.CloseValue > prevQuote.HighValue)
                                {
                                    StockPick sp = new StockPick();
                                    sp.PickDate = quote.QuoteDate;
                                    sp.PickType = pickType;
                                    sp.Symbol = quote.Symbol;
                                    sp.PickKey = pickKey;
                                    dbContext.StockPicks.InsertOnSubmit(sp);

                                    dbContext.SubmitChanges();

                                    startChecking = false;
                                }
                            }
                            else
                                prevQuote = quote;
                        }
                    }
                }


            }
        }

        //public void Test(string symbol, string conn)
        //{
        //    string strSymbol = symbol;
        //    string strConn = conn;


        //}
    }
}
