using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using job_seeker.Src;

namespace job_seeker.Utils;
public class Scraper
{
    public Scraper()
    {

    }

    public List<Job> Scrape(string keywords)
    {
        string url = "https://www.linkedin.com/jobs/search?keywords=" + keywords;
        var chromeOptions = new ChromeOptions();
        var chromeDriverService = ChromeDriverService.CreateDefaultService();
        chromeDriverService.HideCommandPromptWindow = true;
        chromeOptions.AddArgument("headless");
        chromeOptions.AddArgument("disable-gpu");
        using var browser = new ChromeDriver(chromeDriverService, chromeOptions);
        browser.Navigate().GoToUrl(url);
        // TODO: Paginate search results

        // Linkedin
        IList<IWebElement> jobsDetails = browser.FindElements(By.XPath("//*[@id=\"main-content\"]/section[2]/ul/li"));

        List<Job> jobList = new();

        foreach (IWebElement jobResult in jobsDetails.Take(15))
        {
            Dictionary<string, string> jobData = new()
            {
                ["title"] = jobResult.FindElement(By.XPath(".//div/div[2]/h3")).Text,
                ["company"] = jobResult.FindElement(By.XPath(".//div/div[2]/h4/a")).Text,
                ["companyLink"] = jobResult.FindElement(By.XPath(".//div/div[2]/h4/a")).GetAttribute("href"),
                ["location"] = jobResult.FindElement(By.XPath(".//div/div[2]/div/span")).Text,
                ["date"] = jobResult.FindElement(By.XPath(".//div/div[2]/div/time")).Text,
                ["link"] = jobResult.FindElement(By.XPath("div/a")).GetAttribute("href"),
            };
            jobList.Add(new Job(jobData));
        };
        return jobList;
    }
}
