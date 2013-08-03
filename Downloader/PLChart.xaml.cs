using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TradingWizard.StrategyAnalysis.OptionLib;

namespace Downloader
{
    /// <summary>
    /// Interaction logic for PLChart.xaml
    /// </summary>
    public partial class PLChart : Window
    {
        private OptionCombination _optionComb = new OptionCombination();

        private ObservableCollection<OpenOption> _openOptions = new ObservableCollection<OpenOption>();

        private PLSerious _plSerious = new PLSerious();


        public PLChart()
        {
            InitializeComponent();

            this.txtDrawDate.Text = string.Format("{0:yyyy-MM-dd}", DateTime.Today);
            this.lvOptions.ItemsSource = this._openOptions;
        }

        private void DrawPLChart()
        {
            //OpenOption openOption = new OpenOption()
            //{
            //    PurchaseDate = new DateTime(2013, 07, 26),
            //    PurchasePrice = 34.75m,
            //    ContractNo = 2,
            //    Option = new Option()
            //    {
            //        Symbol = "AAPL",
            //        IsCall = true,
            //        ExpiryDate = new DateTime(2014, 04, 14),
            //        Strike = 440
            //    }
            //};

            foreach(OpenOption opt in this._openOptions)
                this._optionComb.Options.Add(opt);

            PLChartUI plChartUI = new PLChartUI(this.cvPLChart, this._optionComb, this.ActualWidth-20);

            //plChartUI.DrawChart(DateTime.Today.AddMonths(6), 440.96m, 0.02m, 0.2513);
        }

        private void btnDrawChart_Click(object sender, RoutedEventArgs e)
        {
            this._optionComb.Options.Clear();
            foreach (OpenOption opt in this._openOptions)
            {
                //opt.PurchaseDate = Convert.ToDateTime(this.txtInterest.Text);
                this._optionComb.Options.Add(opt);
            }

            PLChartUI plChartUI = new PLChartUI(this.cvPLChart, this._optionComb, this.ActualWidth - 20);

            plChartUI.DrawChart(Convert.ToDateTime(this.txtDrawDate.Text), 
                Convert.ToDecimal(this.txtStockPrice.Text), Convert.ToDecimal(this.txtPriceRange.Text),
                Convert.ToDecimal(this.txtInterest.Text), 
                Convert.ToDouble(this.txtVolatility.Text));
        }

        private void btnOptionCMD_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            if (string.Compare(btn.Name, "btnSave", true) == 0)
            {
                OpenOption openOpt = new OpenOption();

                openOpt.ContractNo = Convert.ToInt32(this.txtContract.Text);
                openOpt.PurchasePrice = Convert.ToDecimal(this.txtPrice.Text);
                openOpt.Option = new Option()
                {
                    Strike = Convert.ToDecimal(this.txtStrike.Text),
                    ExpiryDate = Convert.ToDateTime(this.txtExpiryDate.Text),
                    IsCall = this.chkCall.IsChecked.Value
                };

                this._openOptions.Add(openOpt);

                ClearOptionInput();
            }
            else if (string.Compare(btn.Name, "btnClear", true) == 0)
            {
                ClearOptionInput();
            }
            else if (string.Compare(btn.Name, "btnDelete", true) == 0)
            {
                if (this.lvOptions.SelectedItem != null)
                {
                    OpenOption openOpt = this.lvOptions.SelectedItem as OpenOption;
                    this._openOptions.Remove(openOpt);
                }
                ClearOptionInput();
            }

        }

        public void ClearOptionInput()
        {
            this.txtContract.Text = "";
            this.txtExpiryDate.Text = "";
            this.txtPrice.Text = "";
            this.txtStrike.Text = "";
            this.chkCall.IsChecked = false;
        }

        private void lvOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.lvOptions.SelectedItem != null)
            {
                OpenOption openOpt = this.lvOptions.SelectedItem as OpenOption;
                //this._selectedIndex = index;

                this.txtStrike.Text = openOpt.Option.Strike.ToString();
                this.txtExpiryDate.Text = openOpt.Option.ExpiryDate.ToString();
                this.txtPrice.Text = openOpt.PurchasePrice.ToString();
                this.txtContract.Text = openOpt.ContractNo.ToString();
                this.chkCall.IsChecked = openOpt.Option.IsCall;

                //this.lvComponents.ItemsSource = this._symbolRepository.GetIndexComponents(index.IndexName);
                //if (this.lvComponents.Items.Count > 0)
                //    this.tbComponents.Text = string.Format("Total {0} components", this.lvComponents.Items.Count);
                //else
                //    this.tbComponents.Text = "";
            }
        }
    }


}
