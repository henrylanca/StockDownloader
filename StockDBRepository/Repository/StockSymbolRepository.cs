using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDownloader.StockDBRepository
{
    public class StockSymbolRepository
    {
        public void UpdateSymbol(StockSymbol symbol)
        {
            using (StockDataEntities context = new StockDataEntities())
            {
                StockSymbol exsitingSymbol = context.StockSymbols
                    .Where(s => String.Compare(s.Symbol, symbol.Symbol, true) == 0).SingleOrDefault();

                if (exsitingSymbol != null)
                {
                    exsitingSymbol.StockName = symbol.StockName;
                    exsitingSymbol.Sector = symbol.Sector;
                    exsitingSymbol.ETF = symbol.ETF;
                    exsitingSymbol.Country = symbol.Country;
                    exsitingSymbol.HasFuture = symbol.HasFuture;
                    exsitingSymbol.StartDate = symbol.StartDate;
                    exsitingSymbol.EndDate = symbol.EndDate;
                }
                else
                {
                    context.StockSymbols.Add(symbol);
                }

                context.SaveChanges();
            }
        }


    }
}
