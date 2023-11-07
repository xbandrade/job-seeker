using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using job_seeker.Utils;
using job_seeker.Src;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Diagnostics;

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

        private async void AutoSearchDelay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AutoSearchDelay.SelectedIndex == 0)
            {
                TextDisplay.Text = "Select a search delay time before enabling auto search";
                return;
            }

            bool autoSearchDone = true; // TODO: call auto search <<<<<

            string keywords = KeywordsTextBox.Text;
            if (String.IsNullOrEmpty(keywords))
            {
                autoSearchDone = false;

            }
            TextDisplay.Text = $"Searching for \"{keywords}\"...";
            await Task.Run(() =>
            {
                Scraper scraper = new();
                List<Job> results = scraper.Scrape(keywords);
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


        private async void Search_Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(KeywordsTextBox.Text)){
                TextDisplay.Text = "Enter some keywords before searching";
            }
            else
            {
                string text = KeywordsTextBox.Text;
                TextDisplay.Text = $"Searching for \"{text}\"...";
                await Task.Run(() =>
                {
                    Scraper scraper = new();
                    List<Job> results = scraper.Scrape(text);

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
                            TextDisplay.Text = $"Displaying search results for \"{text}\"";
                        }
                        else
                        {
                            TextDisplay.Text = $"No search results found for \"{text}\"";
                        }
                    });
                });
            }
        }
    }
}
