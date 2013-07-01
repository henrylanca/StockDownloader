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

    public sealed class GetPeaks : CodeActivity
    {

        public InArgument<string> MySymbol {get; set; }
        public InArgument<string> MyConnString {get; set;}

        private const int PeakCount = 5;

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override void Execute(CodeActivityContext context)
        {
            string strSymbol = this.MySymbol.Get(context);
            string strConn =this.MyConnString.Get(context);

            using (StockDBDataContext dbContext = new StockDBDataContext(strConn))
            {
               StockPeak lastPeak = (from sp in dbContext.StockPeaks
                                       where sp.Symbol == strSymbol
                                       && sp.TimeFrame == 1
                                       orderby sp.PeakDate descending
                                       select sp).FirstOrDefault();

               DateTime lastDtPeak = DateTime.Now.AddYears(-20);
               decimal lastPeakVal = 0;
               int lastPeakType = 0;

               if (lastPeak != null)
               {
                   lastDtPeak = lastPeak.PeakDate;
                   lastPeakType = lastPeak.PeakType;

                   StockQuote sq = (from a in dbContext.StockQuotes
                                    where a.Symbol == strSymbol
                                    && a.QuoteDate == lastPeak.PeakDate
                                    && a.TimeFrame == lastPeak.TimeFrame
                                    select a).FirstOrDefault();


                   if (sq == null)
                       return;

                    if (lastPeakType < 0)
                        lastPeakVal = sq.LowValue;
                    else if (lastPeakType > 0)
                        lastPeakVal = sq.HighValue;
               }

               

                List<StockQuote> lstQuote = (from sq in dbContext.StockQuotes
                                             where sq.Symbol == strSymbol
                                             && sq.TimeFrame == 1
                                             && sq.QuoteDate >= lastDtPeak  
                                             orderby sq.QuoteDate
                                             select sq).ToList();



                for (int i = 0; i < lstQuote.Count; i++)
                {
                    if(i>=PeakCount  && (i+PeakCount)<=(lstQuote.Count-1))
                    {
                        List<StockQuote> lstTmp = lstQuote.GetRange(i - PeakCount, PeakCount*2+1);

                        decimal minLow = (from a in lstTmp
                                          orderby a.LowValue
                                          select a.LowValue).FirstOrDefault();

                        decimal maxHigh = (from a in lstTmp
                                           orderby a.HighValue descending 
                                           select a.HighValue).FirstOrDefault();

                        if (lstQuote[i].HighValue >= maxHigh)
                        {
                            if (lastPeakType > 0)
                                lastPeakType++;
                            else
                                lastPeakType = 1;

                            StockPeak highPeak = new StockPeak();

                            highPeak.TimeFrame = lstQuote[i].TimeFrame;
                            highPeak.Symbol = lstQuote[i].Symbol;
                            highPeak.PeakDate = lstQuote[i].QuoteDate;
                            highPeak.PeakType = lastPeakType;
                            dbContext.StockPeaks.InsertOnSubmit(highPeak);
                            dbContext.SubmitChanges(); 
 
                        }
                        else if (lstQuote[i].LowValue <= minLow)
                        {
                            if (lastPeakType < 0)
                                lastPeakType--;
                            else
                                lastPeakType = -1;

                            StockPeak highPeak = new StockPeak();

                            highPeak.TimeFrame = lstQuote[i].TimeFrame;
                            highPeak.Symbol = lstQuote[i].Symbol;
                            highPeak.PeakDate = lstQuote[i].QuoteDate;
                            highPeak.PeakType = lastPeakType;
                            dbContext.StockPeaks.InsertOnSubmit(highPeak);
                            dbContext.SubmitChanges(); 
                        }
                    }
                }

            }
        }
    }
}
