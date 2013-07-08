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
    /// Interaction logic for SymbolUpload.xaml
    /// </summary>
    public partial class SymbolUpload : Window
    {
        public SymbolUpload()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Upload Symols into database
        /// The format is
        /// Index, Symbol, Stock Name, Sector, Country, ETF, HasFuture
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            string[] lines = this.txtSymbols.Text.Replace("\r", "")
                .Split(new[] {'\n'},StringSplitOptions.RemoveEmptyEntries);

            List<StockSymbol> txtSymbols = new List<StockSymbol>();
            

            foreach (string line in lines)
            {
                string[] items = line.Split(',');

                if (items.Length != 7)
                {
                    MessageBox.Show(string.Format("Invalid Symbol : {0}", line), "Invalid Symbol",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    int etf = 0;
                    int hasFuture= 0;

                    StockSymbol symbol = new StockSymbol();
                    symbol.Symbol = items[1];
                    symbol.StockName = items[2];
                    symbol.Sector = items[3];
                    symbol.Country = items[4];

                    int.TryParse(items[5],out etf);
                    symbol.ETF = etf;

                    int.TryParse(items[6], out hasFuture);
                    symbol.HasFuture = hasFuture>0;

                }
            }
        }
    }
}
