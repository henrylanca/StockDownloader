﻿using System;
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

        StockChartUI _stockChartUI = null;
        DateTime _chartDate = DateTime.Today;


        public string Symbol
        {
            set
            {
                QuerySymbolInfo(value);
            }
        }


        public StockChart()
        {
            InitializeComponent();

            EnableControls();
        }

        private void btnGetSymbolinfo_Click(object sender, RoutedEventArgs e)
        {
            string symbol = this.txtSymbol.Text;

            QuerySymbolInfo(symbol);
        }

        private void QuerySymbolInfo(string symbol)
        {
            if (!string.IsNullOrEmpty(symbol))
            {
                this._stockSymbol = this._symbolRepository.GetSymbol(symbol);

                if (this._stockSymbol != null)
                {
                    this._chartDate = DateTime.Today;

                    this.txtSymbol.Text = symbol;
                    this.txtFullInfo.Text = string.Format("{0} - {1} ({2:yyyy-MM-dd} - {3:yyyy-MM-dd})",
                        this._stockSymbol.StockName, this._stockSymbol.Sector, this._stockSymbol.StartDate,
                        this._stockSymbol.EndDate);

                    this.rbDay.IsChecked = true;
                    this._stockChartUI = new StockChartUI(this.cvChart, this._stockSymbol.Symbol, 1, this.ActualWidth);

                    DrawChart(1);
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
                //StockChartUI stockChartUI = new StockChartUI(this.cvChart, this._stockSymbol.Symbol, timeFrame, this.ActualWidth);

                this._stockChartUI.TimeFrame = timeFrame;
                this._stockChartUI.DrawChart(this._chartDate);
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
                if (MessageBox.Show(string.Format("Do you want to re-download {0}?",
                    this._stockSymbol.Symbol), "Download Confirmation", MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    StockQuoteRepository quoteRepository = new StockQuoteRepository();
                    quoteRepository.DeleteAllQuotes(this._stockSymbol.Symbol);

                    StockPeakRepository peakRepository = new StockPeakRepository();
                    peakRepository.DeleteAllPeaks(this._stockSymbol.Symbol);

                    StockPickRepository pickRepository = new StockPickRepository();
                    pickRepository.DeleteAllPicks(this._stockSymbol.Symbol);

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

        private void btnMoveChart_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            if (string.Compare(btn.Name, "btnPrev", true) == 0)
            {
                if (this.rbDay.IsChecked == true)
                    this._chartDate = this._chartDate.AddMonths(-1);
                else
                    this._chartDate = this._chartDate.AddMonths(-3);

            }
            else if (string.Compare(btn.Name, "btnNext", true) == 0)
            {
                if (this.rbDay.IsChecked == true)
                    this._chartDate = this._chartDate.AddMonths(1);
                else
                    this._chartDate = this._chartDate.AddMonths(3);

                if (this._chartDate > DateTime.Today)
                    this._chartDate = DateTime.Today;
            }

            this._stockChartUI.DrawChart(this._chartDate);
        }

        private void mnuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;

            switch (menuItem.Name)
            {
                case "mnuCountry":
                    CountryList countryList = new CountryList();
                    countryList.Owner = this;
                    countryList.Show();
                    break;
                case "mnuIndex":
                    IndexList indexList = new IndexList();
                    indexList.Owner = this;
                    indexList.Show();
                    break;
                case "mnuUpload":
                    SymbolUpload upload = new SymbolUpload();
                    upload.Owner = this;
                    upload.Show();
                    break;
                case "mnuPLChart":
                    PLChart plChart = new PLChart();
                    plChart.Owner = this;
                    plChart.Show();
                    break;
                default:
                    break;
            }
        }
    }
}
