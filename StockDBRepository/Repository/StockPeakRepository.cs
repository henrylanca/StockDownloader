using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockDownloader.StockDBRepository
{
    public class StockPeakRepository
    {
        public void DeleteAllPeaks(string symbol)
        {
            using (StockDataEntities context = new StockDataEntities())
            {
                context.Database.ExecuteSqlCommand("Delete from StockPeak Where Symbol= {0}", symbol);
            }
        }
    }
}
