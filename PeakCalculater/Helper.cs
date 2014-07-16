using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PeakCalculater
{
    public class Helper
    {
        /// <summary>
        /// Get Quote Download End Date, the date should be the 
        /// </summary>
        /// <returns></returns>
        public static DateTime GetEndDate(DateTime curDate)
        {
            while (curDate.DayOfWeek != DayOfWeek.Friday)
                curDate=curDate.AddDays(-1);

            return curDate.Date;
        }

        public static void LogExceptions(List<Exception> exceptions)
        {
            string filePath = string.Format(@"{0}\{1:yyyy_MM_dd_HH_mm_ss}.log", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), DateTime.Now);
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (Exception e in exceptions)
                {
                    writer.WriteLine(e.Message);
                    Exception exp = e.InnerException;
                    while (exp != null)
                    {
                        writer.WriteLine(exp.Message);
                        exp = exp.InnerException;
                    }
                }

                writer.Close();
            }
        }
    }
}
