using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Data;
using System.Data.Linq;

using DBModel;

namespace PeakCalculater
{

    public sealed class PickBreakOut : CodeActivity
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
                StockPick lastPick = (from a in dbContext.StockPicks
                                      where a.Symbol == strSymbol
                                      && a.PickType == 2
                                      orderby a.PickDate descending
                                      select a).FirstOrDefault();

                DateTime lastDtPick = DateTime.Now.AddYears(-20);

                if (lastPick != null)
                {
                    lastDtPick = lastPick.PickDate;
                }

                List<StockPeak> lstPeaks = (from a in dbContext.StockPeaks
                                            where a.Symbol == strSymbol
                                            && a.TimeFrame == 1
                                            && a.PeakDate >= lastDtPick.AddMonths(-6)
                                            && a.PeakType >0
                                            orderby a.PeakDate
                                            select a).ToList();

                List<StockQuote> lstQuote = (from a in dbContext.StockQuotes
                                             where a.Symbol == strSymbol
                                             && a.TimeFrame == 1
                                             && a.QuoteDate >= lastDtPick
                                             orderby a.QuoteDate
                                             select a).ToList();
                string orgPickKey = "";
                foreach (StockQuote sq in lstQuote)
                {
                    List<StockPeak> lstRcntPk = (from p in lstPeaks
                                                 where p.PeakDate <= sq.QuoteDate
                                                 && p.PeakDate >= sq.QuoteDate.AddMonths(-6)
                                                 && p.PeakType >0
                                                 orderby p.PeakDate descending
                                                 select p).ToList();

                    StockQuote prevQuote = (from q in lstQuote
                                            where q.QuoteDate < sq.QuoteDate
                                            orderby q.QuoteDate descending
                                            select q).FirstOrDefault();

                    if (prevQuote != null)
                    {
                        decimal maxPeak = 0;
                        foreach (StockPeak sPeak in lstRcntPk)
                        {
                            StockQuote pQuote = (from q in lstQuote
                                            where q.TimeFrame == sPeak.TimeFrame
                                            && q.QuoteDate == sPeak.PeakDate
                                            select q).FirstOrDefault();

                            if (pQuote != null)
                            {
                                if (pQuote.HighValue > maxPeak)
                                    maxPeak = pQuote.HighValue;
                            }
                        }

                        if (prevQuote.CloseValue < maxPeak && sq.CloseValue > maxPeak)
                        {
                                string pickKey = string.Format("{0}_{1:yyyy-MM-dd}_2", sq.Symbol,sq.QuoteDate );

                                if (pickKey != orgPickKey)
                                {
                                    int existFlag = (from p in dbContext.StockPicks
                                                     where p.PickKey == pickKey
                                                     select p.PickKey).Count();

                                    if (existFlag <= 0)
                                    {
                                        StockPick sp = new StockPick();
                                        sp.PickDate = sq.QuoteDate;
                                        sp.PickType = 2;
                                        sp.Symbol = sq.Symbol;
                                        sp.PickKey = pickKey;
                                        dbContext.StockPicks.InsertOnSubmit(sp);

                                        dbContext.SubmitChanges();
                                    }

                                    //Console.WriteLine(string.Format("Pick {0:yyyy-MM-dd}", sq.QuoteDate));
                                    orgPickKey = pickKey;
                                }
                        }
                    }

                }
            }
        }
    }
}
