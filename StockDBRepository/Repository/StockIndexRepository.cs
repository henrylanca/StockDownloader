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

        public List<StockIndex> GetAllIndexes()
        {
            List<StockIndex> indexList = new List<StockIndex>();

            using (StockDataEntities context = new StockDataEntities())
            {
                indexList = context.StockIndexes.OrderBy(i => i.IndexName).ToList();
            }

            return indexList;
        }

        public void DeleteIndex(StockIndex stockIndex)
        {
            using (StockDataEntities context = new StockDataEntities())
            {
                StockIndex existingIndex = context.StockIndexes
                    .Where(i => String.Compare(i.IndexName, stockIndex.IndexName, true) == 0).SingleOrDefault();

                if (existingIndex != null)
                {
                    context.StockIndexes.Remove(existingIndex);

                    context.SaveChanges();
                }


            }
        }

    }
}
