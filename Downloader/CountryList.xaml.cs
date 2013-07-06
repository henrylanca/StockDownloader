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
    /// Interaction logic for CountryList.xaml
    /// </summary>
    public partial class CountryList : Window
    {
        private List<StockCountry> countries;

        public CountryList()
        {
            InitializeComponent();

            StockCountryRepository repository = new StockCountryRepository();
            countries = repository.GetCountryList();

            this.lstCountry.ItemsSource = countries;
        }
    }
}
