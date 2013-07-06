using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDownloader.StockDBRepository
{
    public class StockIndexRepository
    {
        public void UpdateStockIndex(StockIndex stockIndex)
        {
            using (StockDataEntities context = new StockDataEntities())
            {
                StockIndex existingIndex = context.StockIndexes
                    .Where(i => String.Compare(i.IndexName, stockIndex.IndexName, true) == 0).SingleOrDefault();

                if (existingIndex != null)
                {
                    existingIndex.Description = stockIndex.Description;
                }
                else
                {
                    context.StockIndexes.Add(stockIndex);
                }
                
                context.SaveChanges();
            }
        }

        public List<StockCountry> GetAllCountries()
        {
            List<StockCountry> countryList = new List<StockCountry>();

            using (StockDataEntities context = new StockDataEntities())
            {
                countryList = context.StockCountries.ToList();
            }

            return countryList;
        }

    }
}
