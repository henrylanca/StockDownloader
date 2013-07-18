using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using StockDownloader.StockDBRepository;

namespace Downloader
{
    /// <summary>
    /// Interaction logic for StockChart.xaml
    /// </summary>
    public partial class StockChart : Window
    {
        private StockSymbolRepository _symbolRepository = new StockSymbolRepository();

        public StockChart()
        {
            InitializeComponent();
        }


        //private void btnDrawChart_Click(object sender, RoutedEventArgs e)
        //{
        //    //StockChartUI stockChartUI = new StockChartUI(this.cvChart, "G.TO", 2,this.ActualWidth);

        //    //stockChartUI.DrawChart(DateTime.Now);

        //    short timeFrame = 1;

        //    if (this.rbWeek.IsChecked==true)
        //        timeFrame = 2;

        //    this.DrawChart(this.txtSymbol.Text, timeFrame);
        //}



        private void btnGetSymbolinfo_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtSymbol.Text))
            {
                StockSymbol stockSymbol = this._symbolRepository.GetSymbol(this.txtSymbol.Text);

                if (stockSymbol != null)
                {
                    this.txtFullInfo.Text = string.Format("{0} - {1} ({2:yyyy-MM-dd} - {3:yyyy-MM-dd})",
                        stockSymbol.StockName, stockSymbol.Sector, stockSymbol.StartDate,
                        stockSymbol.EndDate);
                    DrawChart(stockSymbol.Symbol, 1);
                }
            }
        }

        private void DrawChart(string symbol, short timeFrame)
        {
            if(!string.IsNullOrEmpty(symbol))
            {
                StockChartUI stockChartUI = new StockChartUI(this.cvChart, symbol, timeFrame, this.ActualWidth);

                stockChartUI.DrawChart(DateTime.Now);
            }
        }

        private void rbTimeFrame_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            short timeFrame = 1;

            switch (rb.Name)
            {
                case "rbWeek":
                    timeFrame=2;
                    break;
                default:
                    timeFrame = 1;
                    break;
            }

            if(!string.IsNullOrEmpty(this.txtSymbol.Text))
                this.DrawChart(this.txtSymbol.Text, timeFrame);
        }
    }
}
