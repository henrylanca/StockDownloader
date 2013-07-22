using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockDownloader.StockDBRepository
{
    public class StockPickRepository
    {
        public void DeleteAllPicks(string symbol)
        {
            using (StockDataEntities context = new StockDataEntities())
            {
                context.Database.ExecuteSqlCommand("Delete from StockPick Where Symbol= {0}", symbol);
            }
        }

        public List<StockPick> GetAllPicks(string symbol)
        {
            using (StockDataEntities context = new StockDataEntities())
            {
                List<StockPick> picks = context.StockPicks
                    .Where(s => (string.Compare(s.Symbol, symbol, true) == 0))
                    .OrderBy(p => p.PickDate).ToList();

                return picks;
            }
        }
    }
}
