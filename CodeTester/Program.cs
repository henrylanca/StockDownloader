using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Data.Linq;
using System.Threading.Tasks;

using DBModel;
using PeakCalculater;
using System.IO;
using System.Reflection;
using System.Threading;
using StockDownloader.StockDBRepository;

namespace CodeTester
{
    class Program
    {
        private static bool _inServiceFlag = false;

        static void Main(string[] args)
        {

            StartDownload();

        }


        private static void StartDownload()
        {
            string strConn = System.Configuration.ConfigurationManager.ConnectionStrings["DBModel.Properties.Settings.StockDataConnectionString"].ToString();
            List<string> lstSymbol = new List<string>();

            DateTime lastEndDate = Helper.GetEndDate(DateTime.Today);

            using (StockDataEntities dbContext = new StockDataEntities())
            {
                lstSymbol = (from s in dbContext.StockSymbols
                             where ((s.EndDate < lastEndDate || s.EndDate==null) /*&& (s.Symbol == "AAPL")*/)
                             orderby s.Symbol
                             select s.Symbol).ToList();
            }

            var exceptions = new Queue<Exception>();

            ParallelOptions pOptions = new ParallelOptions { MaxDegreeOfParallelism = 10 };

            Parallel.ForEach(lstSymbol, pOptions, s =>
            {
                try
                {
                    Console.WriteLine(string.Format("Processing {0}", s));

                    WorkflowInvoker.Invoke(
                        new PeakCalculater.QuoteDownload()
                        {
                            Symbol = s,
                            ConnString = strConn
                        }
                        );

                }
                catch (Exception e)
                {
                    ApplicationException appExp = new ApplicationException(string.Format("Exception happend when processing {0}", s), e);
                    exceptions.Enqueue(appExp);
                }
            }
            );


            if (exceptions.Count > 0)
            {                
                Helper.LogExceptions(exceptions.ToList());
                Console.WriteLine("Check Exception");
                Console.ReadLine();
            }
            else
                Console.WriteLine("Completed");
        }
    }
}
