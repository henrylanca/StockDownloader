using System;
using System.Collections.Generic;
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

        private PLSerious _plSerious = new PLSerious();


        public PLChart()
        {
            InitializeComponent();

            
        }

        private void DrawPLChart()
        {
            OpenOption openOption = new OpenOption()
            {
                PurchaseDate = new DateTime(2013, 07, 26),
                PurchasePrice = 34.75m,
                ContractNo = 2,
                Option = new Option()
                {
                    Symbol = "AAPL",
                    IsCall = true,
                    ExpiryDate = new DateTime(2014, 04, 14),
                    Strike = 440
                }
            };

            this._optionComb.Options.Add(openOption);

            PLChartUI plChartUI = new PLChartUI(this.cvPLChart, this._optionComb, this.ActualWidth-20);

            plChartUI.DrawChart(DateTime.Today.AddMonths(6), 440.96m, 0.02m, 0.2513);

            //OptionCalculator optCalculator = new OptionCalculator();
            //this._plSerious = optCalculator.CalculatePLs(this._optionComb, new DateTime(2013, 05, 20), 300, 600, 0.02m, 0.30);

            //Path path = new Path();
            //path.Stroke = new SolidColorBrush(Colors.Red);
            //path.Data = Geometry.Parse("M 100,100 C 150,120 180, 130, 200,150 M 200,150 C 220,170 240 190 260 230");

            //this.cvPLChart.Children.Add(path);
        }

        private void btnDrawChart_Click(object sender, RoutedEventArgs e)
        {
            DrawPLChart();
        }
    }
}
