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

namespace Downloader
{
    /// <summary>
    /// Interaction logic for StockChart.xaml
    /// </summary>
    public partial class StockChart : Window
    {
        public StockChart()
        {
            InitializeComponent();


        }


        private void btnDraeChart_Click(object sender, RoutedEventArgs e)
        {
            StockChartUI stockChartUI = new StockChartUI(this.cvChart, "IBM", 1,this.ActualWidth);

            stockChartUI.DrawChart(DateTime.Now);
        }
    }
}
