using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Windows.Input;
using job_seeker.Utils;
using job_seeker.Src;

namespace job_seeker
{
    public partial class MainWindow : Window
    {
        private Scraper scraper;
        readonly List<int> delayList;
        readonly Dictionary<string, string> periodDict;
        public MainWindow()
        {
            InitializeComponent();
            scraper = new();
            delayList = new()
            {
                30, 60, 120, 180
            };
            periodDict = new()
            {
                { "", "Any Time" },
                { "3600", "Past Hour" },
                { "86400", "Past 24h" },
                { "604800", "Past Week" },
                { "18144000", "Past Month" },
            };
            List<ComboBoxItem> items = new()
            {
                new ComboBoxItem { Content = "Search Delay", IsEnabled = false },
                new ComboBoxItem { Content = $"{delayList[0]} min" },
                new ComboBoxItem { Content = $"{delayList[1]} min" },
                new ComboBoxItem { Content = $"{delayList[2]} min" },
                new ComboBoxItem { Content = $"{delayList[3]} min" },
            };
            AutoSearchDelay.ItemsSource = items;
            AutoSearchDelay.SelectedIndex = 0;
            items = new();
            foreach (var value in periodDict.Values)
            {
                items.Add(new ComboBoxItem { Content = value });
            }
            PublishDateComboBox.ItemsSource = items;
            PublishDateComboBox.SelectedIndex = 2;
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

        private async void AutoSearchDelay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AutoSearchDelay.SelectedIndex == 0)
            {
                TextDisplay.Text = "Select a search delay time before enabling auto search";
                return;
            }

            bool autoSearchDone = true; // TODO: call auto search <<<<<

            string keywords = KeywordsTextBox.Text;
            string location = LocationTextBox.Text;
            string period = periodDict.ElementAt(PublishDateComboBox.SelectedIndex).Key;
            if (String.IsNullOrEmpty(keywords) && String.IsNullOrEmpty(location))
            {
                autoSearchDone = false;

            }
            TextDisplay.Text = $"Searching for \"{keywords}\"...";
            await Task.Run(() =>
            {
                scraper = new(keywords, location, period);
                List<Job> results = scraper.GetResults();
                PreviousPageButton.IsEnabled = scraper.CurrentPage > 1 && results.Count > 0;
                NextPageButton.IsEnabled = scraper.CurrentPage < scraper.TotalPages;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ResultsList.ItemsSource = results;
                });
                if (results.Count  == 0)
                {
                    autoSearchDone = false;
                }
            });
            
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

        private void HandleLinkClick(object sender, RequestNavigateEventArgs e)
        {
            string url = e.Uri.ToString();
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        private void AddGridColumnBinding(GridView gridView, string header, string? attribute = null, double? width = null)
        {
            if (String.IsNullOrEmpty(attribute))
            {
                attribute = header;
            }
            if (header == "Link")
            {
                DataTemplate dataTemplate = new();
                FrameworkElementFactory textBlockFactory = new(typeof(TextBlock));
                FrameworkElementFactory hyperlinkFactory = new(typeof(Hyperlink));
                hyperlinkFactory.SetBinding(Hyperlink.NavigateUriProperty, new Binding(attribute));
                FrameworkElementFactory runFactory = new(typeof(Run));
                runFactory.SetBinding(Run.TextProperty, new Binding(attribute));
                hyperlinkFactory.AppendChild(runFactory);
                hyperlinkFactory.AddHandler(Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler(HandleLinkClick));
                textBlockFactory.AppendChild(hyperlinkFactory);
                dataTemplate.VisualTree = textBlockFactory;
                GridViewColumn linkColumn = new()
                {
                    Header = header,
                    CellTemplate = dataTemplate,
                    Width = width ?? double.NaN,
                };
                gridView.Columns.Add(linkColumn);
            }
            else
            {
                GridViewColumn titleColumn = new()
                {
                    Header = header,
                    DisplayMemberBinding = new Binding(attribute),
                    Width = width ?? double.NaN,
                };
                gridView.Columns.Add(titleColumn);
            }
        }

        private void UpdateJobLists()
        {
            List<Job> results = scraper.GetResults();
            ResultsList.ItemsSource = results;
        }

        private async void Search_Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(KeywordsTextBox.Text) && string.IsNullOrWhiteSpace(LocationTextBox.Text))
            {
                TextDisplay.Text = "Enter some keywords before searching";
            }
            else
            {
                SearchButton.IsEnabled = false;
                string keywordsText = KeywordsTextBox.Text ?? "";
                string location = LocationTextBox.Text;
                string period = periodDict.ElementAt(PublishDateComboBox.SelectedIndex).Key;
                TextDisplay.Text = $"Searching for \"{keywordsText}\"...";
                await Task.Run(() =>
                {
                    scraper = new(keywordsText, location, period);
                    List<Job> results = scraper.GetResults();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        GridView gridView = new();
                        AddGridColumnBinding(gridView, "Job Title", "Title", 220);
                        AddGridColumnBinding(gridView, "Company", width: 150);
                        AddGridColumnBinding(gridView, "Link", width: 200);
                        AddGridColumnBinding(gridView, "Location");
                        AddGridColumnBinding(gridView, "Date");
                        ResultsList.View = gridView;
                        ResultsList.ItemsSource = results;
                        if (results.Count > 0)
                        {
                            TextDisplay.Text = $"Displaying search results for \"{keywordsText}\"";
                        }
                        else
                        {
                            TextDisplay.Text = $"No search results found for \"{keywordsText}\"";
                        }
                        SearchButton.IsEnabled = true;
                        PreviousPageButton.IsEnabled = scraper.CurrentPage > 1 && results.Count > 0;
                        NextPageButton.IsEnabled = scraper.CurrentPage < scraper.TotalPages;
                        PageLabel.Content = scraper.CurrentPage.ToString();
                    });
                });
            }
        }

        private void PublishDateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void PressSearchButton(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                e.Handled = true;
                Search_Button_Click(sender, e);
            }
        }

        private void KeywordsTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            PressSearchButton(sender, e);
        }

        private void LocationTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            PressSearchButton(sender, e);
        }

        private void PageChangeClick(int pageChange = 1)
        {
            if (scraper != null)
            {
                scraper.CurrentPage += pageChange;
                PreviousPageButton.IsEnabled = scraper.CurrentPage > 1;
                NextPageButton.IsEnabled = scraper.CurrentPage < scraper.TotalPages;
                PageLabel.Content = scraper.CurrentPage.ToString();
            }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            PageChangeClick();
            UpdateJobLists();
        }

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            PageChangeClick(-1);
            UpdateJobLists();
        }
    }
}
