using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StartupJobsParser
{
    public class SjpYieldexScraper : SjpScraper
    {
        private static readonly Uri _defaultUri = new Uri("http://www.yieldex.com/careers/");
        private string _defaultLocation = "New York, NY";

        public override string CompanyName { get { return "Yieldex"; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }
        public override Uri PublicUri { get { return _defaultUri; } }

        public SjpYieldexScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//a[starts-with(@href,'/job/')]"))
            {
                Uri jdUri = new Uri(uri, jdLink.Attributes["href"].Value);
                yield return GetYieldexJd(jdUri);
            }
        }

        private JobDescription GetYieldexJd(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);

            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//h1[@class='entry-title']");

            HtmlNode contentNode = doc.DocumentNode.SelectSingleNode("//div[@class='entry-content']");

            string location = null;
            HtmlDocument description = new HtmlDocument();

            bool captureDescription = false;
            foreach (HtmlNode node in contentNode.ChildNodes)
            {
                const string locationIndicator = "Location:";
                const string descriptionIndicator = "Description:";

                string text = node.InnerText.Trim();

                if (text.StartsWith(locationIndicator))
                {
                    location = text.Substring(locationIndicator.Length).Trim();
                }
                else if (text.StartsWith(descriptionIndicator))
                {
                    captureDescription = true;
                }
                else if (captureDescription)
                {
                    description.DocumentNode.AppendChild(node);
                }
            }

            return new JobDescription()
            {
                SourceUri = jdUri.AbsolutePath,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = location,
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(description.DocumentNode),
                FullHtmlDescription = description.DocumentNode.InnerHtml
            };
        }
    }
}