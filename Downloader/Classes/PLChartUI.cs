using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TradingWizard.StrategyAnalysis.OptionLib;

namespace Downloader
{
    public class PLChartUI
    {
        private Canvas _chart;
        private OptionCombination _optionComb = new OptionCombination();

        private decimal _lowPL;
        private decimal _highPL;
        private decimal _lowPrice;
        private decimal _highPrice;

        public PLChartUI(Canvas chart, OptionCombination optComb, double chartWidth)
        {
            this._chart = chart;
            this._optionComb = optComb;
            this._chart.Width = chartWidth;

            DrawChart(DateTime.Today.AddMonths(1), 441, 0.02m, 0.40);
        }

        public void DrawChart(DateTime drawDate, decimal stockPrice, decimal interest, double volatility )
        {
            OptionCalculator optCalculator = new OptionCalculator();
            PLSerious plSerious = optCalculator.CalculatePLs(this._optionComb, drawDate, stockPrice * 0.6m,
                stockPrice * 1.4m,  interest, volatility);            

            Path path = new Path();
            path.Stroke = new SolidColorBrush(Colors.Red);

            this._lowPL = plSerious.PLPointSerious.OrderBy(p => p.Profit).FirstOrDefault().Profit;
            this._highPL = plSerious.PLPointSerious.OrderByDescending(p => p.Profit).FirstOrDefault().Profit;
            this._lowPrice = plSerious.PLPointSerious.OrderBy(p => p.EquityPrice).FirstOrDefault().EquityPrice;
            this._highPrice = plSerious.PLPointSerious.OrderByDescending(p => p.EquityPrice).FirstOrDefault().EquityPrice;

            decimal margin = (this._highPL - this._lowPL) / 20;
            this._highPL += margin;
            this._lowPL -= margin;

            margin = (this._highPrice - this._lowPrice) / 10;
            this._highPrice += margin;
            this._lowPrice -= margin;
            if (this._lowPrice < 0)
                this._lowPrice = 0;

            int countMod = (plSerious.PLPointSerious.Count - 4) % 3;
            if (countMod == 1)
            {
                plSerious.PLPointSerious.Add(plSerious.PLPointSerious.Last());
            }
            else if (countMod == 2)
            {
                plSerious.PLPointSerious.Add(plSerious.PLPointSerious.Last());
                plSerious.PLPointSerious.Add(plSerious.PLPointSerious.Last());
            }

            StringBuilder pathDate = new StringBuilder();
            int iCount = 0;
            
            foreach (PLPoint pl in plSerious.PLPointSerious)
            {
                int iPosition = iCount % 4;
                Point sPoint = MapPLToChart(pl.EquityPrice, pl.Profit);

                if (iPosition == 0)
                {
                    if (!string.IsNullOrEmpty(pathDate.ToString()))
                    {
                        pathDate.Append(string.Format(" {0},{1} ", sPoint.X, sPoint.Y));
                    }
                    pathDate.Append(string.Format(" M {0},{1}", sPoint.X, sPoint.Y));
                }
                else if (iPosition == 1)
                {
                    pathDate.Append(string.Format(" C {0},{1}", sPoint.X, sPoint.Y));
                }
                else
                {
                    pathDate.Append(string.Format(" {0},{1}", sPoint.X, sPoint.Y));
                }
            }
            path.Data = Geometry.Parse(pathDate.ToString());

            this._chart.Children.Add(path);

        }

        private Point MapPLToChart(decimal price, decimal pl)
        {
            Point cPoint = new Point();
            cPoint.X = (double)((price - this._lowPrice) / (this._highPrice - this._lowPrice)) 
                * this._chart.Width;
            cPoint.Y = this._chart.Height  -
                (double)((pl - this._lowPL) / (this._highPL - this._lowPL)) * this._chart.Height;
            return cPoint;
        }
    }
}
