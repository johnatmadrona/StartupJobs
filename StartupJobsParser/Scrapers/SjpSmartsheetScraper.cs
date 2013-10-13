using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace StartupJobsParser
{
    public class SjpSmartsheetScraper : SjpScraper
    {
        private Uri _defaultUri = new Uri("http://www.smartsheet.com/careers");
        public override string CompanyName { get { return "Smartsheet"; } }
        public override Uri DefaultUri
        {
            get { return _defaultUri; }
        }

        public SjpSmartsheetScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdSection in doc.DocumentNode.SelectNodes("//div[@class='job-container']"))
            {
                string jobTitle = WebUtility.HtmlDecode(jdSection.SelectSingleNode("h3").InnerText).Trim();
                string jdLink = jdSection.SelectSingleNode("div[@class='view-job-description-button-container']/a").Attributes["href"].Value;
                Uri jdUri = new Uri(uri, jdLink);

                yield return GetSmartsheetJd(jobTitle, "Bellevue, WA", jdUri);
            }
        }

        private JobDescription GetSmartsheetJd(string title, string location, Uri jdUri)
        {
            string description = SjpUtils.GetTextFromPdf(jdUri);

            Regex locationRegex = new Regex(@"(?<location>\w+, [A-Z]{2})[^\w]");
            Match m = locationRegex.Match(description);
            if (m.Success)
            {
                location = m.Groups["location"].Value;
            }

            string cleanTextDescription = description.Replace('\n', ' ').Replace('\r', ' ');
            cleanTextDescription = Regex.Replace(cleanTextDescription, "\\s+", " ");

            return new JobDescription()
            {
                SourceUri = TryCreateTrackedLink(jdUri),
                Company = CompanyName,
                Title = title,
                Location = location,
                FullTextDescription = cleanTextDescription,
                FullHtmlDescription = WebUtility.HtmlEncode(description)
            };
        }
    }
}