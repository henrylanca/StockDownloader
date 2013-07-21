using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Data.Linq;
using System.Threading.Tasks;

using DBModel;
using PeakCalculater;


namespace CodeTester
{
    class Program
    {
        static void Main(string[] args)
        {
            string strConn = System.Configuration.ConfigurationManager.ConnectionStrings["DBModel.Properties.Settings.StockDataConnectionString"].ToString(); 
            List<string> lstSymbol = new List<string>();

            using (StockDBDataContext dbContext = new StockDBDataContext(strConn))
            {
                lstSymbol = (from s in dbContext.StockSymbols
                             //where s.Symbol =="GLW"d
                             orderby s.Symbol
                             select s.Symbol).ToList(); 
            }

            var exceptions = new Queue<Exception>();

            ParallelOptions pOptions = new ParallelOptions { MaxDegreeOfParallelism = 10 };

            Parallel.ForEach(lstSymbol,pOptions, s =>
            {
                Console.WriteLine(string.Format("Processing {0}", s));

                try
                {
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
                    ApplicationException appExp = new ApplicationException(string.Format("Exception happend when processing {0}",s),e);
                    exceptions.Enqueue(appExp);
                }
            }
            );

            //using (StockDBDataContext dbContext = new StockDBDataContext(strConn))
            //{
            //    try
            //    {
            //        dbContext.CommandTimeout = 0;
            //        dbContext.ExecuteCommand("Exec DBSync");
            //    }
            //    catch (Exception exp)
            //    {
            //        ApplicationException appExp = new ApplicationException("Exception happend when processing DBSync", exp);
            //        exceptions.Enqueue(appExp);
            //    }

            //}

            if (exceptions.Count > 0)
            {
                foreach (Exception e in exceptions)
                {
                    Console.WriteLine(e.Message);
                    Exception exp = e.InnerException;
                    while (exp != null)
                    {
                        Console.WriteLine(exp.Message);
                        exp = exp.InnerException;
                    }
                }

                Console.Read();
            }
            else
                Console.WriteLine("Completed");

        }
    }
}
