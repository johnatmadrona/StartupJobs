using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpRedfinScraper : SjpScraper
    {
        private Uri _defaultUri = new Uri("http://hire.jobvite.com/CompanyJobs/Careers.aspx?k=JobListing&c=qf49Vfw7&jvresize=http%3a%2f%2fwww.redfin.com%2fabout%2fjobs%2fframeresize&v=1"); 
        public override string CompanyName { get { return "Redfin"; } }
        public override Uri DefaultUri { get { return _defaultUri; } }

        public SjpRedfinScraper(string storageDirPath, ISjpIndex index)
            : base(storageDirPath, index)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
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

                yield return GetRedfinJd(new Uri(actualJdLink));
            }
        }

        private JobDescription GetRedfinJd(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);

            string title = doc.DocumentNode.SelectSingleNode("//h1").InnerText;
            string location = doc.DocumentNode.SelectSingleNode("//h2").InnerText;
            string description = doc.DocumentNode.SelectSingleNode("//div[@class='jobDesc']").InnerHtml;

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = WebUtility.HtmlDecode(title).Trim(),
                Location = WebUtility.HtmlDecode(location).Trim(),
                FullTextDescription = WebUtility.HtmlDecode(description).Trim(),
                FullHtmlDescription = description
            };
        }
    }
}