using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDownloader.StockDBRepository
{
    public class StockQuoteRepository
    {
        public List<StockQuote> GetQuotes(string symbol, short timeFrame)
        {
            using (StockDataEntities context = new StockDataEntities())
            {
                return context.StockQuotes.Where(q => (string.Compare(q.Symbol, symbol, true) == 0 
                    && q.TimeFrame == timeFrame))
                    .OrderBy(q => q.QuoteDate).ToList();
            }
        }

        public List<StockQuote> GetQuotes(string symbol, short timeFrame, DateTime startDate, DateTime endDate)
        {
            using (StockDataEntities context = new StockDataEntities())
            {
                return context.StockQuotes.Where(q => (string.Compare(q.Symbol, symbol, true) == 0 
                    && q.TimeFrame == timeFrame && q.QuoteDate>=startDate && q.QuoteDate<=endDate))
                    .OrderBy(q => q.QuoteDate).ToList();
            }
        }
    }
}
