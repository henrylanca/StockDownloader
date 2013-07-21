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
using System.Activities;

using StockDownloader.StockDBRepository;
using PeakCalculater;

namespace Downloader
{
    /// <summary>
    /// Interaction logic for StockChart.xaml
    /// </summary>
    public partial class StockChart : Window
    {
        private StockSymbolRepository _symbolRepository = new StockSymbolRepository();

        private StockSymbol _stockSymbol = null;

        public StockChart()
        {
            InitializeComponent();

            EnableControls();
        }


        private void btnGetSymbolinfo_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtSymbol.Text))
            {
                this._stockSymbol = this._symbolRepository.GetSymbol(this.txtSymbol.Text);

                if (this._stockSymbol != null)
                {
                    this.txtFullInfo.Text = string.Format("{0} - {1} ({2:yyyy-MM-dd} - {3:yyyy-MM-dd})",
                        this._stockSymbol.StockName, this._stockSymbol.Sector, this._stockSymbol.StartDate,
                        this._stockSymbol.EndDate);

                    this.rbDay.IsChecked = true;

                    DrawChart( 1);
                }
                else
                {
                    MessageBox.Show(string.Format("Cannot find infromation for {0}", this.txtSymbol.Text));
                }
            }

            EnableControls();
        }

        private void DrawChart(short timeFrame)
        {
            if(this._stockSymbol!=null)
            {
                StockChartUI stockChartUI = new StockChartUI(this.cvChart, this._stockSymbol.Symbol, timeFrame, this.ActualWidth);

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

            this.DrawChart(timeFrame);
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            if (this._stockSymbol != null)
            {
                StockQuoteRepository quoterepository = new StockQuoteRepository();
                quoterepository.DeleteAllQuotes(this._stockSymbol.Symbol);

                this._stockSymbol.EndDate = new DateTime(1990, 1, 1);

                List<StockSymbol> stockSymbols = new List<StockSymbol>();
                stockSymbols.Add(this._stockSymbol);

                this._symbolRepository.UpdateSymbols(stockSymbols);



                string s = this._stockSymbol.Symbol;
                string strConn = System.Configuration.ConfigurationManager.ConnectionStrings["StockDataDB"].ToString();

                try
                {
                    WorkflowInvoker.Invoke(
                        new PeakCalculater.QuoteDownload()
                        {
                            Symbol = s,
                            ConnString = strConn
                        }
                        );
                    MessageBox.Show("Quote Downloading completed successfully");

                }
                catch (Exception exp)
                {
                    MessageBox.Show(exp.Message, "Download Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void EnableControls()
        {
            if (this._stockSymbol != null)
            {
                this.pnlStockInfo.Visibility = Visibility.Visible;
            }
            else
            {
                this.pnlStockInfo.Visibility = Visibility.Collapsed;
            }
        }
    }
}
