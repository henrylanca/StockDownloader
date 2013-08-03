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

        private decimal _lowPL = Decimal.MaxValue;
        private decimal _highPL = Decimal.MinValue;
        private decimal _lowPrice = Decimal.MaxValue;
        private decimal _highPrice = Decimal.MinValue;

        public PLChartUI(Canvas chart, OptionCombination optComb, double chartWidth)
        {
            this._chart = chart;
            this._optionComb = optComb;
            this._chart.Width = chartWidth;

            //DrawChart(DateTime.Today.AddMonths(1), 441, 0.02m, 0.40);
        }

        public void DrawChart(DateTime drawDate, decimal stockPrice, decimal priceRange, decimal interest, double volatility )
        {
            this._chart.Children.Clear();

            OptionCalculator optCalculator = new OptionCalculator();
            List<PLSerious> plList = new List<PLSerious>();

            plList.Add(optCalculator.CalculatePLs(this._optionComb, drawDate.AddMonths(1), stockPrice * (1-priceRange),
                stockPrice * (1+priceRange),  interest, volatility));

            plList.Add(optCalculator.CalculatePLs(this._optionComb, drawDate.AddMonths(3), stockPrice * (1 - priceRange),
                stockPrice * (1 + priceRange), interest, volatility));

            plList.Add(optCalculator.CalculatePLs(this._optionComb, drawDate.AddMonths(6), stockPrice * (1 - priceRange),
                stockPrice * (1 + priceRange), interest, volatility));  


            foreach (PLSerious plSerious in plList)
            {
                decimal lowPL = plSerious.PLPointSerious.OrderBy(p => p.Profit).FirstOrDefault().Profit;
                decimal highPL = plSerious.PLPointSerious.OrderByDescending(p => p.Profit).FirstOrDefault().Profit;
                decimal lowPrice = plSerious.PLPointSerious.OrderBy(p => p.EquityPrice).FirstOrDefault().EquityPrice;
                decimal highPrice = plSerious.PLPointSerious.OrderByDescending(p => p.EquityPrice).FirstOrDefault().EquityPrice;

                if (this._lowPL > lowPL)
                    this._lowPL = lowPL;

                if (this._lowPrice > lowPrice)
                    this._lowPrice = lowPrice;

                if (this._highPL < highPL)
                    this._highPL = highPL;

                if (this._highPrice < highPrice)
                    this._highPrice = highPrice;
            }

            decimal margin = (this._highPL - this._lowPL) / 20;
            this._highPL += margin;
            this._lowPL -= margin;

            //Draw PL lines
            Line xLine = new Line();
            Point xPoint = MapPLToChart(stockPrice, 0);
            xLine.Stroke = new SolidColorBrush(Colors.Black);
            xLine.StrokeThickness = 3;
            xLine.X1 = 0;
            xLine.X2 = this._chart.Width;
            xLine.Y1 = xPoint.Y;
            xLine.Y2 = xPoint.Y;
            this._chart.Children.Add(xLine);

            decimal startPL = margin;
            while (startPL <= this._highPL)
            {
                DrawPLLIne(stockPrice, startPL);

                startPL += margin;
            }

            startPL = margin * -1;
            while (startPL >= this._lowPL)
            {
                DrawPLLIne(stockPrice, startPL);

                startPL -= margin;
            }

            //Draw Price Lines
            decimal priceMargin = (this._highPrice - this._lowPrice) / 15;
            decimal startPrice = stockPrice;
            while (startPrice <= this._highPrice)
            {
                DrowPriceLine(startPrice, 0);
                startPrice += priceMargin;
            }
            startPrice = stockPrice;
            while (startPrice >= this._lowPrice)
            {
                DrowPriceLine(startPrice, 0);
                startPrice -= priceMargin;
            }



            foreach (PLSerious plSerious in plList)
            {
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

                    //iCount++;
                }

                Path path = new Path();
                path.Stroke = new SolidColorBrush(Colors.Red);
                
                path.Data = Geometry.Parse(pathDate.ToString());

                this._chart.Children.Add(path);
            }


        }

        private void DrawPLLIne(decimal stockPrice, decimal startPL)
        {
            Line plLine = new Line();
            Point plPoint = MapPLToChart(stockPrice, startPL);
            plLine.Stroke = new SolidColorBrush(Colors.Black);
            plLine.StrokeThickness = 1;
            plLine.X1 = 0;
            plLine.X2 = this._chart.Width;
            plLine.Y1 = plPoint.Y;
            plLine.Y2 = plPoint.Y;
            this._chart.Children.Add(plLine);

            TextBox txtPL = new TextBox();
            txtPL.Text = string.Format("{0:C}", startPL);
            txtPL.FontSize = 8;
            txtPL.TextAlignment = TextAlignment.Center;
            txtPL.Foreground = new SolidColorBrush(Colors.Blue);
            Canvas.SetLeft(txtPL, this._chart.Width / 2);
            Canvas.SetTop(txtPL, plPoint.Y - 10);
            this._chart.Children.Add(txtPL);
        }

        private void DrowPriceLine(decimal stockPrice, decimal PL)
        {
            Line priceLine = new Line();
            Point plPoint = MapPLToChart(stockPrice, 0);
            priceLine.Stroke = new SolidColorBrush(Colors.Black);
            priceLine.StrokeThickness = 1;
            priceLine.X1 = plPoint.X;
            priceLine.X2 = plPoint.X;
            priceLine.Y1 = 0;
            priceLine.Y2 = this._chart.Height;
            this._chart.Children.Add(priceLine);

            TextBox txtPrice = new TextBox();
            txtPrice.Text = string.Format("{0:C}", stockPrice);
            txtPrice.FontSize = 8;
            txtPrice.TextAlignment = TextAlignment.Center;
            txtPrice.Foreground = new SolidColorBrush(Colors.Blue);
            Canvas.SetLeft(txtPrice, plPoint.X );
            Canvas.SetTop(txtPrice, this._chart.Height-10);
            this._chart.Children.Add(txtPrice);
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
