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

    public sealed class PickReversal : CodeActivity
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
                                      && a.PickType == 1
                                      orderby a.PickDate descending
                                      select a).FirstOrDefault();

                DateTime lastDtPick = DateTime.Now.AddYears(-10);
 
                if (lastPick != null)
                {
                    lastDtPick = lastPick.PickDate;
                }

                List<StockPeak> lstPeaks = (from a in dbContext.StockPeaks
                                            where a.Symbol == strSymbol
                                            && a.TimeFrame == 1
                                            && a.PeakDate >= lastDtPick.AddMonths(-6) 
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
                                                  && p.PeakDate >= sq.QuoteDate.AddDays(-45)  
                                                  orderby p.PeakDate descending
                                                  select p).ToList();
                    
                    if (lstRcntPk.Count >= 3)
                    {
                        if (lstRcntPk[0].PeakType < 0)
                        {
                            decimal p1 = (from q in lstQuote
                                          where q.QuoteDate == lstRcntPk[0].PeakDate
                                          select q.LowValue).FirstOrDefault();


                            decimal p2 = 0;
                            decimal p3 = 0;

                            for (int i = 1; i < lstRcntPk.Count; i++)
                            {
                                if (lstRcntPk[i].PeakType < 0 && p3==0)
                                {
                                    p3 = (from q in lstQuote
                                          where q.QuoteDate == lstRcntPk[i].PeakDate
                                          select q.LowValue).FirstOrDefault();
                                }

                                if (lstRcntPk[i].PeakType >0 && p2 == 0)
                                {
                                    p2 = (from q in lstQuote
                                          where q.QuoteDate == lstRcntPk[i].PeakDate
                                          select q.HighValue).FirstOrDefault();
                                }

                                if (p2 != 0 && p3 != 0)
                                    break;
                            }

                            if (p2!=0 && p3 !=0)
                            {
                                if (p1 <= p3 && sq.CloseValue > p2)
                                {
                                    string pickKey = string.Format("{0}_{1:yyyy-MM-dd}_{2:yyyy-MM-dd}", sq.Symbol,
                                        lstRcntPk[0].PeakDate, lstRcntPk[1].PeakDate);

                                    if (pickKey != orgPickKey)
                                    {
                                        int existFlag = (from p in dbContext.StockPicks
                                                         where p.PickKey == pickKey
                                                         select p.PickKey).Count();

                                        if (existFlag <= 0)
                                        {
                                            StockPick sp = new StockPick();
                                            sp.PickDate = sq.QuoteDate;
                                            sp.PickType = 1;
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
    }
}
