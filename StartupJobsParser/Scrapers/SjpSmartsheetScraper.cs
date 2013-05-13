using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace StartupJobsParser
{
    public class SjpSmartsheetScraper : SjpScraper
    {
        public override string CompanyName { get { return "Smartsheet"; } }
        public override string DefaultUri
        {
            get { return "http://www.smartsheet.com/careers"; }
        }

        public SjpSmartsheetScraper(string storageDirPath, ISjpIndex index)
            : base(storageDirPath, index)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(string uri)
        {
            Uri baseUri = new Uri(uri);

            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdSection in doc.DocumentNode.SelectNodes("//div[@class='job-container']"))
            {
                string jobTitle = WebUtility.HtmlDecode(jdSection.SelectSingleNode("h3").InnerText);
                string jdLink = jdSection.SelectSingleNode("div[@class='view-job-description-button-container']/a").Attributes["href"].Value;
                Uri jdUri = new Uri(baseUri, jdLink);

                yield return GetSmartsheetJd(jobTitle, jdUri.AbsoluteUri);
            }
        }

        private JobDescription GetSmartsheetJd(string title, string jdLink)
        {
            string description = SjpUtils.GetTextFromPdf(jdLink);

            string location = "Bellevue, WA";
            Regex locationRegex = new Regex(@"(?<location>\w+, [A-Z]{2})[^\w]");
            Match m = locationRegex.Match(description);
            if (m.Success)
            {
                location = m.Groups["location"].Value;
            }

            return new JobDescription()
            {
                SourceUri = jdLink,
                Company = CompanyName,
                Title = title,
                Location = location,
                FullDescription = description
            };
        }
    }
}