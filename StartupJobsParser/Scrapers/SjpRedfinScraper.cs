using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpRedfinScraper : SjpScraper
    {
        public override string CompanyName { get { return "Redfin"; } }
        //public override string DefaultUri { get { return "http://www.redfin.com/about/open-jobs"; } }
        public override string DefaultUri { get { return "http://hire.jobvite.com/CompanyJobs/Careers.aspx?k=JobListing&c=qf49Vfw7&jvresize=http%3a%2f%2fwww.redfin.com%2fabout%2fjobs%2fframeresize&v=1"; } }

        public SjpRedfinScraper(string storageDirPath, ISjpIndex index)
            : base(storageDirPath, index)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(string uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//a[contains(@href, '/open-jobs?')]"))
            {
                // Get the jobvite ID from the link
                Uri jdUri = new Uri(jdLink.Attributes["href"].Value);
                string jvi = jdUri.Query.Split('=')[1];

                string actualJdLink = string.Format(
                    "http://hire.jobvite.com/CompanyJobs/Careers.aspx?k=JobListing&c=qf49Vfw7&jvresize=http%3a%2f%2fwww.redfin.com%2fabout%2fjobs%2fframeresize&j={0}&v=1",
                    WebUtility.UrlEncode(jvi)
                    );

                yield return GetRedfinJd(actualJdLink);
            }
        }

        private JobDescription GetRedfinJd(string jdLink)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdLink);

            string title = doc.DocumentNode.SelectSingleNode("//h1").InnerText;
            string location = doc.DocumentNode.SelectSingleNode("//h2").InnerText;
            string description = doc.DocumentNode.SelectSingleNode("//div[@class='jobDesc']").InnerText;

            return new JobDescription()
            {
                SourceUri = jdLink,
                Company = CompanyName,
                Title = WebUtility.HtmlDecode(title),
                Location = WebUtility.HtmlDecode(location),
                FullDescription = WebUtility.HtmlDecode(description)
            };
        }
    }
}