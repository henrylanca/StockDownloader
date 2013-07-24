using System;
using System.Collections.Generic;
using System.Globalization;
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
        private decimal _volumeHigh;


        private List<StockQuote> _stockQuotes;
        private List<StockQuote> _stockWeekQuotes;
        private StockQuoteRepository _quoteRepository;
        private StockPickRepository _pickRepository;

        private List<StockQuote> _chartQuote;
        private List<StockPick> _pickList;

        public short TimeFrame
        {
            get
            {
                return this._timeFrame;
            }

            set
            {
                this._timeFrame = value;
            }
        }

        public StockChartUI(Canvas chart, string symbol, short timeFrame, double windowsWidth)
        {            
            this._chart = chart;
            this._symbol = symbol;
            this._timeFrame = timeFrame;

            this._chart.Width = windowsWidth - 30;

            this._quoteRepository = new StockQuoteRepository();
            this._pickRepository = new StockPickRepository();
            this._stockQuotes = this._quoteRepository.GetQuotes(this._symbol,1)
                .OrderByDescending(q => q.QuoteDate).ToList();
            this._stockWeekQuotes = this._quoteRepository.GetQuotes(this._symbol,2)
                .OrderByDescending(q => q.QuoteDate).ToList();

            this._pickList = this._pickRepository.GetAllPicks(this._symbol);

        }

        public void DrawChart(DateTime endDate)
        {
            this._chart.Children.Clear();

            this._endDate = endDate;
            if (this._timeFrame == 1)
            {
                this._startDate = this._endDate.AddYears(-1);

                this._chartQuote = this._stockQuotes.Where(q => (q.QuoteDate >= this._startDate &&
                    q.QuoteDate <= this._endDate)).OrderBy(q => q.QuoteDate).ToList();
            }
            else
            {
                this._startDate = this._endDate.AddYears(-3);
                this._chartQuote = this._stockWeekQuotes.Where(q => (q.QuoteDate >= this._startDate &&
                    q.QuoteDate <= this._endDate)).OrderBy(q => q.QuoteDate).ToList();
            }

            
            if (this._chartQuote.Count > 0)
            {
                this._rangeHigh = this._chartQuote.OrderByDescending(q => q.HighValue).FirstOrDefault().HighValue ;
                this._rangeLow = this._chartQuote.OrderBy(q => q.LowValue).FirstOrDefault().LowValue ;

                decimal margin = (this._rangeHigh - this._rangeLow) / 20;
                this._rangeHigh += margin;
                this._rangeLow -= margin;

                StockQuote highVolimeQuote = this._chartQuote.OrderByDescending(q => q.Volume).FirstOrDefault();
                if (highVolimeQuote != null)
                    this._volumeHigh = highVolimeQuote.Volume * 1.1m;
                else
                    this._volumeHigh = 100;

                decimal priceGap = (this._rangeHigh - this._rangeLow) / 10;
                decimal startPrice = this._rangeLow;
                int xCount = this._chartQuote.Count;
                while (startPrice <= this._rangeHigh)
                {
                    Point lineLeft = MapQuoteToChart(1, startPrice);
                    Point lineRight = MapQuoteToChart(xCount, startPrice);

                    Line priceLine = new Line();
                    priceLine.Stroke = new SolidColorBrush(Colors.White);
                    priceLine.StrokeThickness = 1;
                    priceLine.X1 = lineLeft.X;
                    priceLine.X2 = lineRight.X;
                    priceLine.Y1 = lineLeft.Y;
                    priceLine.Y2 = lineRight.Y;
                    this._chart.Children.Add(priceLine);

                    TextBlock txtPrice = new TextBlock();
                    txtPrice.Text = string.Format("{0:C}", startPrice);
                    txtPrice.FontSize = 10;                   
                    txtPrice.TextAlignment = TextAlignment.Center;
                    txtPrice.Foreground = new SolidColorBrush(Colors.White);
                    Canvas.SetLeft(txtPrice, lineLeft.X+10);
                    Canvas.SetTop(txtPrice, lineLeft.Y-10);
                    this._chart.Children.Add(txtPrice);

                    startPrice += priceGap;
                }

                DateTime dtLine = DateTime.MinValue;
                
                int iPos = 1;
                foreach (StockQuote quote in this._chartQuote)
                {
                    bool drawLineFlag = false;

                    if (this._timeFrame == 1)
                    {
                        if (quote.QuoteDate.Month != dtLine.Month)
                            drawLineFlag = true;
                    }
                    else if (this._timeFrame == 2)
                    {
                        if (GetQuarter(quote.QuoteDate) != GetQuarter(dtLine))
                            drawLineFlag = true;
                    }

                    if (drawLineFlag)
                    {
                        Point lineLow = MapQuoteToChart(iPos, this._rangeLow);
                        Point lineHigh = MapQuoteToChart(iPos, this._rangeHigh);

                        Line dateLine = new Line();
                        dateLine.Stroke = new SolidColorBrush(Colors.White);
                        dateLine.StrokeThickness = 1;
                        dateLine.X1 = lineLow.X;
                        dateLine.X2 = lineHigh.X;
                        dateLine.Y1 = lineLow.Y;
                        dateLine.Y2 = lineHigh.Y;
                        this._chart.Children.Add(dateLine);

                        dtLine = quote.QuoteDate;

                        TextBlock txtDt = new TextBlock();
                        txtDt.Text = string.Format("{0:yy-MM-dd}", quote.QuoteDate);
                        txtDt.FontSize = 10;
                        txtDt.TextAlignment = TextAlignment.Center;
                        txtDt.Foreground = new SolidColorBrush(Colors.White);
                        Canvas.SetLeft(txtDt, lineLow.X);
                        Canvas.SetTop(txtDt, lineLow.Y);
                        this._chart.Children.Add(txtDt);
                    }

                    //Draw Candle Stick
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

                    Point highPoint = MapQuoteToChart(iPos, quote.HighValue);
                    Point lowPoint = MapQuoteToChart(iPos, quote.LowValue);

                    Line ln = new Line();
                    ln.Stroke = rec.Fill;
                    ln.X1 = highPoint.X;
                    ln.X2 = lowPoint.X;
                    ln.Y1 = highPoint.Y;
                    ln.Y2 = lowPoint.Y;
                    this._chart.Children.Add(ln);


                    //Draw Volumn Bar
                    Point vOpenPoint = MapVolumeToChart(iPos, 0m);
                    Point vclosePoint = MapVolumeToChart(iPos, quote.Volume);
                    vOpenPoint.X--;
                    vclosePoint.X++;

                    Rectangle vrec = new Rectangle();
                    vrec.Width = Math.Abs(vOpenPoint.X - vclosePoint.X) + 1;
                    vrec.Height = Math.Abs(vOpenPoint.Y - vclosePoint.Y);
                    Canvas.SetLeft(vrec, Math.Min(vOpenPoint.X, vclosePoint.X));
                    Canvas.SetTop(vrec, Math.Min(vOpenPoint.Y, vclosePoint.Y));
                    if (quote.CloseValue >= quote.OpenValue)
                        vrec.Fill = new SolidColorBrush(Colors.Green);
                    else
                        vrec.Fill = new SolidColorBrush(Colors.Red);
                    this._chart.Children.Add(vrec);


                    //Draw Stock Pick Numner
                    List<StockPick> picks = this._pickList
                        .Where(p => p.PickDate == quote.QuoteDate).ToList();
                    if (this._timeFrame == 2)
                    {
                        picks = this._pickList
                            .Where(p => (p.PickDate.Year == quote.QuoteDate.Year &&
                                GetWeekofYear(p.PickDate) == GetWeekofYear(quote.QuoteDate))).ToList();
                    }

                    string strPick = string.Empty;
                    foreach (StockPick pick in picks)
                    {
                        if(!string.IsNullOrEmpty(strPick))
                            strPick +=",";
                        strPick += pick.PickType.ToString();
                    }

                    if (picks.Count > 0)
                    {
                        TextBlock txtPick = new TextBlock();
                        txtPick.Text = strPick;
                        txtPick.FontSize = 12;
                        txtPick.TextAlignment = TextAlignment.Center;
                        txtPick.Foreground = new SolidColorBrush(Colors.White);
                        Canvas.SetLeft(txtPick, highPoint.X);
                        Canvas.SetTop(txtPick, highPoint.Y - 10);
                        this._chart.Children.Add(txtPick);
                    }
                    
                    iPos++;

                }
            }


        }

        private Point MapQuoteToChart(int x, decimal price)
        {
            Point cPoint = new Point();
            cPoint.X = x * (this._chart.Width-10) / this._chartQuote.Count;
            cPoint.Y = this._chart.Height*.8 - 
                (double)((price - this._rangeLow) / (this._rangeHigh - this._rangeLow)) * this._chart.Height* 0.8;
            return cPoint;
        }

        private Point MapVolumeToChart(int x, decimal volumn)
        {
            Point cPoint = new Point();
            cPoint.X = x * (this._chart.Width - 10) / this._chartQuote.Count;
            cPoint.Y = this._chart.Height  -
                (double)((volumn - 0) / (this._volumeHigh )) * this._chart.Height * 0.15;
            return cPoint;
        }

        private int GetQuarter(DateTime quoteDate)
        {
            return (quoteDate.Month - 1) / 3 + 1;
        }

        private int GetWeekofYear(DateTime quoteDate)
        {
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            System.Globalization.Calendar cal = dfi.Calendar;

            return cal.GetWeekOfYear(quoteDate, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
        }

       
    }
}
