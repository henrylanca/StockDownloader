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
        private StockSymbolRepository _symbolRepository = new StockSymbolRepository();

        private StockIndex _selectedIndex = null;

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

                            CleanInput();
                            LoadIndexes();
                        }
                    }
                    break;
                case "btnDelete":
                    if (ValidInput())
                    {
                        if (MessageBox.Show("Do you want to Delete Index?", "Delete Index",
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            this._repository.DeleteIndex(
                                new StockIndex()
                                {
                                    IndexName = this.txtName.Text,
                                    Description = this.txtDesc.Text,
                                    CountryCode = (this.cbCountry.SelectedItem as StockCountry).Code
                                });

                            CleanInput();
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

        private void lvButton_Click(object sender, RoutedEventArgs e)
        {
            StockSymbol symbol = ((ListViewItem)this.lvComponents.ContainerFromElement((Button)sender)).Content as StockSymbol;

            if (MessageBox.Show(string.Format("Do you want to delete {0} from index?", symbol.Symbol), "Remove Symbol conformation",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                this._repository.RemoveComponentfromIndex(this._selectedIndex.IndexName, symbol);
                this.lvComponents.ItemsSource = this._symbolRepository.GetIndexComponents(this._selectedIndex.IndexName);
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

        private void lvIndexes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.lvIndexes.SelectedItem != null)
            {
                StockIndex index = this.lvIndexes.SelectedItem as StockIndex;
                this._selectedIndex = index;

                this.txtName.Text = index.IndexName;
                this.txtDesc.Text = index.Description;
                this.cbCountry.SelectedValue = index.CountryCode;

                this.lvComponents.ItemsSource = this._symbolRepository.GetIndexComponents(index.IndexName);
                if (this.lvComponents.Items.Count > 0)
                    this.tbComponents.Text = string.Format("Total {0} components", this.lvComponents.Items.Count);
                else
                    this.tbComponents.Text = "";
            }
        }

        private void lvComponents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.lvComponents.SelectedItem != null)
            {
                StockSymbol stocksymbol = this.lvComponents.SelectedItem as StockSymbol;

                if (this.Owner != null)
                {
                    StockChart chartWnd = this.Owner as StockChart;
                    chartWnd.Symbol = stocksymbol.Symbol;
                }
            }
        }
    }
}
