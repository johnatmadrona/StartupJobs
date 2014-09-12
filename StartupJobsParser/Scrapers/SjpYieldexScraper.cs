using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StartupJobsParser
{
    public class SjpYieldexScraper : SjpScraper
    {
        private static readonly Uri _defaultUri = new Uri("http://www.yieldex.com/");
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
            foreach (HtmlNode jdNode in doc.DocumentNode.SelectNodes("//div[@class='lightbox careers']/article/div"))
            {
                yield return GetYieldexJd(jdNode);
            }
        }

        private JobDescription GetYieldexJd(HtmlNode jdNode)
        {
            HtmlNode titleNode = jdNode.SelectSingleNode("h2");
            HtmlNode locationNode = titleNode.SelectSingleNode("span");
            titleNode.RemoveChild(locationNode);

            HtmlNode descriptionNode = jdNode.SelectSingleNode("div");

            string location = SjpUtils.GetCleanTextFromHtml(locationNode);
            if (string.Compare(location, "New York City", true) == 0)
            {
                location = "New York, NY";
            }

            return new JobDescription()
            {
                SourceUri = PublicUri.AbsolutePath,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = location,
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}