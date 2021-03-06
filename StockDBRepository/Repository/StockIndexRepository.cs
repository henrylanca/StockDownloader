﻿using System;
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

        public void AddComponentsToIndex(string index, List<StockSymbol> symbols)
        {
            using (StockDataEntities context = new StockDataEntities())
            {
                StockIndex stockIndex = context.StockIndexes
                    .Where(i => string.Compare(i.IndexName, index, true) == 0).FirstOrDefault();

                if (stockIndex != null)
                {
                    foreach(StockSymbol symbol in symbols)
                    {
                        var existingSymbol = context.StockSymbols
                            .Where(s => string.Compare(s.Symbol, symbol.Symbol, true)==0).SingleOrDefault();
                        if (existingSymbol != null)
                        {
                            stockIndex.StockSymbols.Add(existingSymbol);
                        }
                        else
                        {
                            throw new ApplicationException(string.Format("Cannot find symbol: {0}", symbol.Symbol));
                        }
                    }
                    
                    context.SaveChanges();
                }
            }
        }

        public void RemoveComponentfromIndex(string index, StockSymbol symbol)
        {
            using (StockDataEntities context = new StockDataEntities())
            {
                StockIndex stockIndex = context.StockIndexes
                    .Where(i => string.Compare(i.IndexName, index, true) == 0).FirstOrDefault();

                if (stockIndex != null)
                {

                    var existingSymbol = context.StockSymbols
                        .Where(s => string.Compare(s.Symbol, symbol.Symbol, true) == 0).SingleOrDefault();
                    if (existingSymbol != null)
                    {
                        stockIndex.StockSymbols.Remove(existingSymbol);
                    }
                    else
                    {
                        throw new ApplicationException(string.Format("Cannot find symbol: {0}", symbol.Symbol));
                    }

                    context.SaveChanges();
                }
            }
        }

    }
}
