using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using StockDownloader.StockDBRepository;

namespace Downloader
{
    public class StockChartUI
    {
        private Canvas _chart;
        private string _symbol;
        private short _timeFrame;
        private DateTime _startDate;
        private DateTime _endDate;

        private decimal _rangeHigh;
        private decimal _rangeLow;


        private List<StockQuote> _stockQuotes;
        private StockQuoteRepository _quoteRepository;

        private List<StockQuote> _chartQuote;

        public StockChartUI(Canvas chart, string symbol, short timeFrame, double windowsWidth)
        {            
            this._chart = chart;
            this._symbol = symbol;
            this._timeFrame = timeFrame;

            this._chart.Width = windowsWidth - 30;

            this._quoteRepository = new StockQuoteRepository();
            this._stockQuotes = this._quoteRepository.GetQuotes(this._symbol,
                this._timeFrame)
                .OrderByDescending(q => q.QuoteDate).ToList();

        }

        public void DrawChart(DateTime endDate)
        {
            this._chart.Children.Clear();

            this._endDate = endDate;
            if (this._timeFrame == 1)
                this._startDate = this._endDate.AddYears(-1);
            else
                this._startDate = this._endDate.AddYears(-5);

            this._chartQuote = this._stockQuotes.Where(q => (q.QuoteDate >= this._startDate &&
                q.QuoteDate <= this._endDate)).OrderBy(q => q.QuoteDate).ToList();

            if (this._chartQuote.Count > 0)
            {
                //this._candleWidth = (int)(this._chart.Width / this._chartQuote.Count) - 2;

                //if (this._candleWidth <= 0)
                //    this._candleWidth = 1;

                this._rangeHigh = this._chartQuote.OrderByDescending(q => q.HighValue).FirstOrDefault().HighValue ;
                this._rangeLow = this._chartQuote.OrderBy(q => q.HighValue).FirstOrDefault().LowValue ;

                decimal margin = (this._rangeHigh - this._rangeLow) / 20;
                this._rangeHigh += margin;
                this._rangeLow -= margin;


                int iPos = 1;
                foreach (StockQuote quote in this._chartQuote)
                {
                    Point OpenPoint = MapQuoteToChart(iPos, quote.OpenValue);
                    Point closePoint = MapQuoteToChart(iPos, quote.CloseValue);
                    OpenPoint.X--;
                    closePoint.X++;

                    Rectangle rec = new Rectangle();
                    rec.Width = Math.Abs(OpenPoint.X - closePoint.X)+1;
                    rec.Height = Math.Abs(OpenPoint.Y - closePoint.Y);
                    Canvas.SetLeft(rec, Math.Min(OpenPoint.X, closePoint.X));
                    Canvas.SetTop(rec, Math.Min(OpenPoint.Y, closePoint.Y));
                    if(quote.CloseValue>= quote.OpenValue)
                        rec.Fill = new SolidColorBrush(Colors.Green);
                    else
                        rec.Fill = new SolidColorBrush(Colors.Red);

                    this._chart.Children.Add(rec);

                    iPos++;

                }
            }


        }

        private Point MapQuoteToChart(int x, decimal price)
        {
            Point cPoint = new Point();
            cPoint.X = x * (this._chart.Width-10) / this._chartQuote.Count;
            cPoint.Y = this._chart.Height - 
                (double)((price - this._rangeLow) / (this._rangeHigh - this._rangeLow)) * this._chart.Height;
            return cPoint;
        }
       
    }
}
