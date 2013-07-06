using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDownloader.StockDBRepository
{
    public class StockCountryRepository
    {
        public void UpdateCountry(StockCountry country)
        {
            using (StockDataEntities context = new StockDataEntities())
            {
                StockCountry sCountry = context.StockCountries
                    .Where(c => (string.Compare(c.Code, country.Code, true) == 0)).SingleOrDefault();


                if (sCountry != null)
                    sCountry.FullName = country.FullName;
                else
                    context.StockCountries.Add(country);

                context.SaveChanges();
            }
        }

        public List<StockCountry> GetCountryList()
        {
            using (StockDataEntities context = new StockDataEntities())
            {
                return context.StockCountries.ToList();
            }
        }

    }
}
