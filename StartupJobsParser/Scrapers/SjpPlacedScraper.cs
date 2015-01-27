using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpPlacedScraper : SjpScraper
    {
        private static readonly Uri _defaultUri = new Uri("http://www.placed.com/about/careers");

        public override string CompanyName { get { return "Placed"; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }
        public override Uri PublicUri { get { return _defaultUri; } }

        public SjpPlacedScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//a[starts-with(@href,'/careers/')]"))
            {
                yield return GetPlacedJd(new Uri(uri, jdLink.Attributes["href"].Value));
            }
        }

        private JobDescription GetPlacedJd(Uri jdUri)
        {
            HtmlNode jdNode = SjpUtils.GetHtmlDoc(jdUri).DocumentNode;

            HtmlNode titleNode = jdNode.SelectSingleNode("//h1[@class='banner_title']");
            HtmlNode baseDescriptionNode = jdNode.SelectSingleNode("//div[@class='job_description']");
            HtmlNode requirementsNode = jdNode.SelectSingleNode("//div[@class='requirements']");

            HtmlDocument description = new HtmlDocument();
            description.LoadHtml("<div></div>");
            description.DocumentNode.AppendChild(baseDescriptionNode);
            description.DocumentNode.AppendChild(requirementsNode);

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = "Seattle, WA",
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(description.DocumentNode),
                FullHtmlDescription = description.DocumentNode.InnerHtml
            };
        }
    }
}