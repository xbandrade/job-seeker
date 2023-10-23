using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace job_seeker
{
    public class Scraper
    {
        public Scraper()
        {

        }

        public List<string> Scrape(string keywords)
        {
            string url = "https://www.linkedin.com/jobs/search?keywords=" + keywords;
            var chromeOptions = new ChromeOptions();
            // chromeOptions.AddArgument("headless");
            // chromeOptions.AddArgument("disable-gpu");
            List<string> jobTitles = new();
            using (var browser = new ChromeDriver(chromeOptions))
            {
                browser.Navigate().GoToUrl(url);
                // FIXME
                /*IWebElement ulElement = browser.FindElement(By.ClassName("obs-search__results-list"));
                IReadOnlyCollection<IWebElement> liElements = ulElement.FindElements(By.TagName("li"));
                foreach (var liElement in liElements)
                {
                    IWebElement h3Element = liElement.FindElement(By.ClassName("base-search-card__title"));
                    jobTitles.Add(h3Element.Text);
                }*/
            }
            return jobTitles;
        }
    }
}
