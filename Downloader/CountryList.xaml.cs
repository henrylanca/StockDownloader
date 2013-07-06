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
    /// Interaction logic for CountryList.xaml
    /// </summary>
    public partial class CountryList : Window
    {
        private ObservableCollection<StockCountry> countries = new ObservableCollection<StockCountry>();
        private StockCountryRepository repository = new StockCountryRepository();

        public CountryList()
        {
            InitializeComponent();

            this.lstCountry.ItemsSource = countries;

            this.LoadCountries();

        }

        private void lstCountry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.lstCountry.SelectedItem != null)
            {
                StockCountry country = this.lstCountry.SelectedItem as StockCountry;
                this.txtCode.Text = country.Code;
                this.txtName.Text = country.FullName;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Name)
            {
                case "btnSave":
                    if (this.ValidateCountry())
                    {
                        if (MessageBox.Show("Do you want to Save the changes?", "Save", MessageBoxButton.YesNo)
                            == MessageBoxResult.Yes)
                        {
                            repository.UpdateCountry( new StockCountry() 
                            { Code=this.txtCode.Text, FullName=this.txtName.Text });

                            LoadCountries();
                            ClearInput();
                        }
                    }
                    break;
                case "btnDelete":
                    if (this.ValidateCountry())
                    {
                        if (MessageBox.Show("Do you want to Delete the country?", "Delete", MessageBoxButton.YesNo)
                            == MessageBoxResult.Yes)
                        {
                            repository.DeleteCountry(new StockCountry() { Code = this.txtCode.Text, FullName = this.txtName.Text });

                            LoadCountries();
                            ClearInput();
                        }
                    }
                    break;
                case "btnClear":
                    ClearInput();
                    break;
                default:
                    break;
            }
        }

        private void LoadCountries()
        {
            this.countries.Clear();

            var countryList = repository.GetCountryList();
            foreach (var country in countryList)
                countries.Add(country);

            
        }

        private bool ValidateCountry()
        {
            if (this.txtCode.Text.Trim().Length <= 0)
            {
                MessageBox.Show("Invalid Code", "Invalid Country Code", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (this.txtName.Text.Trim().Length <= 0)
            {
                MessageBox.Show("Invalid Code", "Invalid Country Name", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else
            {
                return true;
            }


        }

        private void ClearInput()
        {
            this.txtCode.Text = "";
            this.txtName.Text = "";
        }
    }
}
