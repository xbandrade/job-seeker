using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using JobSeeker.Src;
using System;
using System.Threading;

namespace JobSeeker.Utils;
public class Scraper
{
    private readonly string url;
    private readonly ChromeOptions chromeOptions;
    private readonly ChromeDriverService chromeDriverService;
    private readonly ChromeDriver browser;
    private List<Dictionary<string, string>>? scrapedJobDataList;
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public IList<IWebElement>? jobsDetailsList;

    public Scraper(string keywords = "", string location = "worldwide", string period = "")
    {
        url = $"https://www.linkedin.com/jobs/search?keywords={keywords}&location={location}&f_TPR=r{period}";
        chromeOptions = new ChromeOptions();
        chromeDriverService = ChromeDriverService.CreateDefaultService();
        chromeDriverService.HideCommandPromptWindow = true;
        chromeOptions.AddArgument("headless");
        chromeOptions.AddArgument("disable-gpu");
        CurrentPage = 1;
        using (browser = new ChromeDriver(chromeDriverService, chromeOptions))
        {
            ScrollPageDown();
            Scrape();
        }
    }
    private void ScrollPageDown()
    {
        var jsExecutor = (IJavaScriptExecutor)browser;
        for (int i = 0; i < 5; ++i)
        {
            jsExecutor.ExecuteScript("window.scrollBy(0, document.body.scrollHeight);");
            Thread.Sleep(500);
        }
    }
    private void Scrape()
    {
        browser.Navigate().GoToUrl(this.url);
        scrapedJobDataList = new List<Dictionary<string, string>>();
        var jobElements = browser.FindElements(By.XPath("//*[@id=\"main-content\"]/section[2]/ul/li"));
        foreach (var jobElement in jobElements)
        {
            try
            {
                var jobData = new Dictionary<string, string>
                {
                    ["title"] = jobElement.FindElement(By.XPath(".//div/div[2]/h3")).Text,
                    ["company"] = jobElement.FindElement(By.XPath(".//div/div[2]/h4/a")).Text,
                    ["companyLink"] = jobElement.FindElement(By.XPath(".//div/div[2]/h4/a")).GetAttribute("href"),
                    ["location"] = jobElement.FindElement(By.XPath(".//div/div[2]/div/span")).Text,
                    ["date"] = jobElement.FindElement(By.XPath(".//div/div[2]/div/time")).Text,
                    ["link"] = jobElement.FindElement(By.XPath("div/a")).GetAttribute("href"),
                };
                scrapedJobDataList.Add(jobData);
            }
            catch (NoSuchElementException)
            {
                continue;
            }
        }
        PageSize = 15;
        TotalPages = (int)Math.Ceiling((double)scrapedJobDataList.Count / PageSize);
    }
    public List<Job> GetResults()
    {
        List<Job> jobList = new();
        if (scrapedJobDataList != null)
        {
            int startIndex = (CurrentPage - 1) * PageSize;
            int endIndex = Math.Min(CurrentPage * PageSize, scrapedJobDataList.Count);
            for (int i = startIndex; i < endIndex; ++i)
            {
                jobList.Add(new Job(scrapedJobDataList[i]));
            }
        }
        browser.Quit();
        chromeDriverService.Dispose();
        return jobList;
    }
}
