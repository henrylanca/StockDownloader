using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDownloader.StockDBRepository
{
    public class StockSymbolRepository
    {
        public void UpdateSymbols(List<StockSymbol> symbols)
        {
            using (StockDataEntities context = new StockDataEntities())
            {
                foreach (StockSymbol symbol in symbols)
                {
                    StockSymbol exsitingSymbol = context.StockSymbols
                        .Where(s => String.Compare(s.Symbol, symbol.Symbol, true) == 0).SingleOrDefault();

                    if (exsitingSymbol != null)
                    {
                        exsitingSymbol.StockName = symbol.StockName;
                        exsitingSymbol.Sector = string.IsNullOrEmpty(symbol.Sector) ? exsitingSymbol.Sector : symbol.Sector;
                        exsitingSymbol.ETF = symbol.ETF.HasValue ? symbol.ETF : exsitingSymbol.ETF;
                        exsitingSymbol.Country = string.IsNullOrEmpty(symbol.Country) ? exsitingSymbol.Country : symbol.Country;
                        exsitingSymbol.HasFuture = symbol.HasFuture.HasValue ? symbol.HasFuture : exsitingSymbol.HasFuture;
                        exsitingSymbol.StartDate = symbol.StartDate.HasValue ? symbol.StartDate : exsitingSymbol.StartDate;
                        exsitingSymbol.EndDate = symbol.EndDate.HasValue ? symbol.EndDate : exsitingSymbol.EndDate;
                    }
                    else
                    {
                        context.StockSymbols.Add(symbol);
                    }
                }

                context.SaveChanges();
            }
        }


    }
}
