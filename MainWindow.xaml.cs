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
using JobSeeker.Utils;
using JobSeeker.Src;

namespace JobSeeker;
public partial class MainWindow : Window
{
    private Scraper scraper;
    readonly Dictionary<string, string> periodDict;
    private bool searchOk;
    private bool autoSearchCall;
    public MainWindow()
    {
        InitializeComponent();
        searchOk = false;
        autoSearchCall = false;
        scraper = new();
        periodDict = new()
        {
            { "", "Any Time" },
            { "3600", "Past Hour" },
            { "86400", "Past 24h" },
            { "604800", "Past Week" },
            { "18144000", "Past Month" },
        };
        List<ComboBoxItem> items = new();
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

    private void AutoSearchCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        autoSearchCall = true;
        Search_Button_Click(sender, e);
    }

    private void AutoSearchCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        TextDisplay.Text = "Auto Search Disabled";
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

    private void AddDefaultColumns(GridView gridView)
    {
        AddGridColumnBinding(gridView, "Job Title", "Title", 220);
        AddGridColumnBinding(gridView, "Company", width: 150);
        AddGridColumnBinding(gridView, "Link", width: 200);
        AddGridColumnBinding(gridView, "Location");
        AddGridColumnBinding(gridView, "Date");
    }

    private async void Search_Button_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(KeywordsTextBox.Text))
        {
            TextDisplay.Text = "Enter some keywords before searching";
            searchOk = false;
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
                    AddDefaultColumns(gridView);
                    ResultsList.View = gridView;
                    ResultsList.ItemsSource = results;
                    if (results.Count > 0)
                    {
                        TextDisplay.Text = $"Displaying search results for \"{keywordsText}\"";
                        searchOk = true;
                    }
                    else
                    {
                        TextDisplay.Text = $"No search results found for \"{keywordsText}\"";
                        searchOk = false;
                    }
                    SearchButton.IsEnabled = true;
                    PreviousPageButton.IsEnabled = scraper.CurrentPage > 1 && results.Count > 0;
                    NextPageButton.IsEnabled = scraper.CurrentPage < scraper.TotalPages;
                    PageLabel.Content = scraper.CurrentPage.ToString();
                });
            });
        }
        if (autoSearchCall)
        {
            autoSearchCall = false;
            if (searchOk)
            {
                // TODO: add autosearch logic
                DateTime now = DateTime.Now;
                int minsToAdd = 60;
                DateTime nextSearch = now.AddMinutes(minsToAdd);
                TextDisplay.Text = $"Auto Search Done!\nNext Search: {nextSearch}";
            }
            else
            {
                AutoSearchCheckBox.IsChecked = false;
                TextDisplay.Text = "No results found in auto search!";
            }
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
