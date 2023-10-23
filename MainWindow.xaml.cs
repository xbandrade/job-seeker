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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace job_seeker
{
    public partial class MainWindow : Window
    {
        readonly List<int> delayList;
        public MainWindow()
        {
            InitializeComponent();
            delayList = new()
            {
                15, 30, 60, 120, 180
            };
            List<ComboBoxItem> items = new()
            {
                new ComboBoxItem { Content = "Search Delay", IsEnabled = false },
                new ComboBoxItem { Content = $"{delayList[0]} min" },
                new ComboBoxItem { Content = $"{delayList[1]} min" },
                new ComboBoxItem { Content = $"{delayList[2]} min" },
                new ComboBoxItem { Content = $"{delayList[3]} min" },
                new ComboBoxItem { Content = $"{delayList[4]} min" },
            };
            AutoSearchDelay.ItemsSource = items;
            AutoSearchDelay.SelectedIndex = 0;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        private void AutoSearchDelay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AutoSearchDelay.SelectedIndex == 0)
            {
                TextDisplay.Text = "Select a search delay time first!";
                return;
            }
            bool autoSearchDone = true; // TODO: call auto search <<<<<
            if (autoSearchDone)
            {
                DateTime now = DateTime.Now;
                int minsToAdd = delayList[AutoSearchDelay.SelectedIndex - 1];
                DateTime nextSearch = now.AddMinutes(minsToAdd); 
                TextDisplay.Text = $"Auto Search Done!\nNext Search: {nextSearch}";
            }
            else {
                TextDisplay.Text = "Auto Search Failed!";
            }
        }

        private void AutoSearchCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            HandleAutoSearchCheckBox();
        }

        private void AutoSearchCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            HandleAutoSearchCheckBox();
        }

        private void HandleAutoSearchCheckBox()
        {
            bool isCheckBoxChecked = AutoSearchCheckBox.IsChecked == true;
            AutoSearchDelay.IsEnabled = isCheckBoxChecked;
            if (isCheckBoxChecked == true)
            {
                TextDisplay.Text = "Auto Search Enabled!\n Select a Delay Time";
            }
            else
            {
                TextDisplay.Text = "Auto Search Disabled";
                AutoSearchDelay.SelectedIndex = 0;
            }
        }

        private void Search_Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(KeywordsTextBox.Text)){
                TextDisplay.Text = "Enter some keywords before searching";
            }
            else
            {
                string text = KeywordsTextBox.Text;
                TextDisplay.Text = $"Searching for: {text}";
                Scraper scraper = new();
                List<string> results = scraper.Scrape(text);
                ResultsList.ItemsSource = results;
                if (results.Count > 0)
                {
                    TextDisplay.Text = "Results for " + text;
                }
                else
                {
                    TextDisplay.Text = "No results found for " + text;
                }
            }
        }
    }
}
