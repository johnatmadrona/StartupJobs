using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace StartupJobsParser
{
    public class SjpZ2Scraper : SjpScraper
    {
        private static readonly Uri _defaultUri = new Uri("http://www.z2.com/careers");

        public override string CompanyName { get { return "Z2"; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }
        public override Uri PublicUri { get { return _defaultUri; } }

        public SjpZ2Scraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//a[@class='career-inner']"))
            {
                yield return GetZ2LiveJd(new Uri(uri, jdLink.Attributes["href"].Value));
            }
        }

        private JobDescription GetZ2LiveJd(Uri jdUri)
        {
            HtmlNode jdNode = SjpUtils.GetHtmlDoc(jdUri).DocumentNode;

            HtmlNode titleNode = jdNode.SelectSingleNode("//h3[@class='career-listing-title']");

            const string LocationPrefix = "Location:";
            string location = jdNode.SelectSingleNode("//strong[@class='location']").InnerText;
            int locationOffset = location.IndexOf(LocationPrefix) + LocationPrefix.Length;
            location = location.Substring(locationOffset);

            HtmlNode descriptionNode = jdNode.SelectSingleNode("//div[@class='ee-rich-text']");

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = SjpUtils.GetCleanTextFromHtml(location),
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}