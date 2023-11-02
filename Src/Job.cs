using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace job_seeker.Src;

public class Job
{
    public string Title { get; set; }
    public string Company { get; set; }
    public string CompanyLink { get; set; }
    public string Location { get; set; }
    public string Date { get; set; }
    public string Link { get; set; }

    public Job(Dictionary<string, string> jobData)
    {
        if (jobData.ContainsKey("title"))
        {
            Title = jobData["title"];
        }
        else
        {
            throw new ArgumentException("Error while retrieving the job title");
        }
        Company = jobData.ContainsKey("company") ? jobData["company"] : string.Empty;
        CompanyLink = jobData.ContainsKey("companyLink") ? jobData["companyLink"] : string.Empty;
        Location = jobData.ContainsKey("location") ? jobData["location"] : string.Empty;
        Date = jobData.ContainsKey("date") ? jobData["date"] : string.Empty;
        Link = jobData.ContainsKey("link") ? jobData["link"] : string.Empty;
    }
}
