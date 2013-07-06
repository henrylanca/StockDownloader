using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for IndexList.xaml
    /// </summary>
    public partial class IndexList : Window
    {
        private ObservableCollection<StockIndex> _stockIndexs = new ObservableCollection<StockIndex>();
        private StockIndexRepository _repository = new StockIndexRepository();
        private StockCountryRepository _countryRepository = new StockCountryRepository();

        public IndexList()
        {
            InitializeComponent();

            List<StockCountry> countryList = this._countryRepository.GetCountryList();
            this.cbCountry.ItemsSource = countryList;

            this.lvIndexes.ItemsSource = this._stockIndexs;

            LoadIndexes();
        }

        private void LoadIndexes()
        {
            this._stockIndexs.Clear();

            var indexes = this._repository.GetAllIndexes();

            foreach (var index in indexes)
                this._stockIndexs.Add(index);
                        
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            switch (button.Name)
            {
                case "btnSave":
                    if (ValidInput())
                    {
                        if (MessageBox.Show("Do you want to save changes?", "Save Index",
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            this._repository.UpdateStockIndex(
                                new StockIndex()
                                {
                                    IndexName = this.txtName.Text,
                                    Description = this.txtDesc.Text,
                                    CountryCode = (this.cbCountry.SelectedItem as StockCountry).Code
                                });

                            LoadIndexes();
                        }
                    }
                    break;
                case "btnClear":
                    CleanInput();
                    break;
                default:
                    break;
            }
        }

        private void CleanInput()
        {
            this.txtName.Text = "";
            this.txtDesc.Text = "";
            this.cbCountry.SelectedIndex = -1;
        }

        private bool ValidInput()
        {
            if (this.txtName.Text.Trim().Length <= 0)
            {
                MessageBox.Show("Invalid Name", "Invalid Index Name", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (this.cbCountry.SelectedItem==null)
            {
                MessageBox.Show("Invalid Country", "Invalid Index Country", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
