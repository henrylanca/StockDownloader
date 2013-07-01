using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.VisualBasic;

using DBModel;

namespace PeakCalculater
{

    public sealed class DownloadSymbolInfo : CodeActivity
    {
        public InArgument<string> MySymbol { get; set; }
        public InArgument<string> MyConnString { get; set; }

        private enum InfoOrder { Symbol = 0, Divident = 1, MarketCap = 2, PriceSales = 3, PriceBooks = 4, PE = 5, PEG = 6, DividentPayDay = 7, ShortRatio = 8, DividentYield = 9, AvgVolumn = 10 };
        private enum NumberDegree { Unknown = -1, Billion = 0, Million = 1, Thousand = 2 };

        class InfoTag
        {
            public static string Symbol = "s";
            public static string Divident = "d";
            public static string MarketCap = "j1";
            public static string PriceSales = "p5";
            public static string PriceBooks = "p6";
            public static string PE = "r";
            public static string PEG = "r5";
            public static string DivedentPayDay = "r1";
            public static string ShortRatio = "s7";
            public static string DividentYield = "y";
            public static string AvgVolume = "a2";
        };

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override void Execute(CodeActivityContext context)
        {
            string symbol = context.GetValue(this.MySymbol);
            string strConn = this.MyConnString.Get(context);

            string requestURL = "http://finance.yahoo.com/d/quotes.csv?s=" + symbol + "&f=" + InfoTag.Symbol
                + InfoTag.Divident + InfoTag.MarketCap + InfoTag.PriceSales + InfoTag.PriceBooks + InfoTag.PE
                + InfoTag.PEG + InfoTag.DivedentPayDay + InfoTag.ShortRatio + InfoTag.DividentYield
                + InfoTag.AvgVolume;

            List<string> lstInfo = HttpLib.GetHttpRespsonse (requestURL);

            using (StockDBDataContext dbContext = new StockDBDataContext(strConn))
            {
                int iExist = (from q in dbContext.StockSymbols
                              where q.Symbol == symbol
                              select q).Count();

                if(iExist<=0)
                    throw new ApplicationException("Cannot find Symbol: " + symbol);

                foreach (string strInfo in lstInfo)
                {
                    string[] items = strInfo.Split(',');

                    if (items[(int)InfoOrder.Symbol].Trim().Replace("\"", "").ToUpper() == symbol.ToUpper())
                    {

                            var infos = from i in dbContext.StockInformations
                                        where i.Symbol == symbol
                                        select i;

                            List<StockInformation> lstStockInfo = infos.ToList<StockInformation>();

                            StockInformation stockInfo = new StockInformation();

                            if (lstStockInfo.Count == 1)
                                stockInfo = lstStockInfo[0];

                            stockInfo.Symbol = symbol;

                            #region 1.Parsing Divident
                            if (Information.IsNumeric(items[(int)InfoOrder.Divident]))
                                stockInfo.Divident = Convert.ToDecimal(items[(int)InfoOrder.Divident]);
                            #endregion

                            #region 2.Parsing MarketCap
                            NumberDegree degree = NumberDegree.Unknown;
                            string marketCap = items[(int)InfoOrder.MarketCap].Replace("\"", "").ToUpper();
                            if (marketCap.Contains("B"))
                            {
                                degree = NumberDegree.Billion;
                                marketCap = marketCap.Replace("B", "");
                            }
                            else if (marketCap.Contains("M"))
                            {
                                degree = NumberDegree.Million;
                                marketCap = marketCap.Replace("M", "");
                            }
                            else if (marketCap.Contains("K"))
                            {
                                degree = NumberDegree.Thousand;
                                marketCap = marketCap.Replace("K", "");
                            }

                            if (Information.IsNumeric(marketCap))
                            {
                                stockInfo.MarKetCap = Convert.ToDecimal(marketCap);

                                switch (degree)
                                {
                                    case NumberDegree.Billion:
                                        stockInfo.MarKetCap *= 100000000;
                                        break;
                                    case NumberDegree.Million:
                                        stockInfo.MarKetCap *= 1000000;
                                        break;
                                    case NumberDegree.Thousand:
                                        stockInfo.MarKetCap *= 1000;
                                        break;
                                }
                            }
                            #endregion

                            #region 3.Parsing Price/Sales
                            if (Information.IsNumeric(items[(int)InfoOrder.PriceSales]))
                                stockInfo.PriceSales = Convert.ToDecimal(items[(int)InfoOrder.PriceSales]);
                            #endregion

                            #region 4.Parsing Price/Books
                            if (Information.IsNumeric(items[(int)InfoOrder.PriceBooks]))
                                stockInfo.PriceBooks = Convert.ToDecimal(items[(int)InfoOrder.PriceBooks]);
                            #endregion

                            #region 5.Parsing PE
                            if (Information.IsNumeric(items[(int)InfoOrder.PE]))
                                stockInfo.PE = Convert.ToDecimal(items[(int)InfoOrder.PE]);
                            #endregion

                            #region 6.Parsing PEG
                            if (Information.IsNumeric(items[(int)InfoOrder.PEG]))
                                stockInfo.PEG = Convert.ToDecimal(items[(int)InfoOrder.PEG]);
                            #endregion

                            #region 6.Parsing Dividents Payday
                            string dividentDay = DateTime.Now.Year.ToString() + " " + items[(int)InfoOrder.DividentPayDay].Replace("\"", "");
                            if (Information.IsDate(dividentDay))
                                stockInfo.DivedentPayDay = Convert.ToDateTime(dividentDay);
                            #endregion

                            #region 7.Parsing Short Ratio
                            if (Information.IsNumeric(items[(int)InfoOrder.ShortRatio]))
                                stockInfo.ShortRatio = Convert.ToDecimal(items[(int)InfoOrder.ShortRatio]);
                            #endregion

                            #region 8.Parsing Divident Yield
                            if (Information.IsNumeric(items[(int)InfoOrder.DividentYield]))
                                stockInfo.DividentYield = Convert.ToDecimal(items[(int)InfoOrder.DividentYield]);
                            #endregion

                            #region 9.Parsing Divident Yield
                            if (Information.IsNumeric(items[(int)InfoOrder.DividentYield]))
                                stockInfo.DividentYield = Convert.ToDecimal(items[(int)InfoOrder.DividentYield]);
                            #endregion

                            #region 10.Parsing Average Volume
                            if (Information.IsNumeric(items[(int)InfoOrder.AvgVolumn]))
                                stockInfo.AvgVolume = Convert.ToInt32(items[(int)InfoOrder.AvgVolumn]);
                            #endregion

                            if (lstStockInfo.Count != 1)
                                dbContext.StockInformations.InsertOnSubmit(stockInfo);

                            dbContext.SubmitChanges();

                    }
                }
            }

        }
    }


}
