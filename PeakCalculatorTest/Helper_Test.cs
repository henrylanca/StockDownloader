using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeakCalculater;
using System.Collections.Generic;



namespace PeakCalculatorTest
{
    [TestClass]
    public class Helper_Test
    {
        [TestMethod]
        public void Test_GetEndDate()
        {
            DateTime curDate = new DateTime(2014, 07, 02);

            curDate = Helper.GetEndDate(curDate);

            Assert.AreEqual(curDate, new DateTime(2014, 06, 27));
        }

        [TestMethod]
        public void Test_LogExceptions()
        {
            List<Exception> exceptions = new List<Exception>();

            exceptions.Add(new Exception("Exception 1"));
            exceptions.Add(new Exception("Exception 2", new Exception("InnerException 1")));

            Helper.LogExceptions(exceptions);


            
        }
    }
}
