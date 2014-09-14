using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace StartupJobsParser
{
    public class SjpHighspotScraper : SjpScraper
    {
        private static readonly Uri _defaultUri = new Uri("https://www.highspot.com/about/careers/");

        public override string CompanyName { get { return "Highspot"; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }
        public override Uri PublicUri { get { return _defaultUri; } }

        public SjpHighspotScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode linkNode in doc.DocumentNode.SelectNodes("//article[@id='careers']//a[contains(@href,'/about/careers/')]"))
            {
                Uri linkUri = new Uri(uri, linkNode.Attributes["href"].Value);
                yield return GetPeachJd(linkUri);
            }
        }

        private JobDescription GetPeachJd(Uri jdUri)
        {
            HtmlNode doc = SjpUtils.GetHtmlDoc(jdUri).DocumentNode;
            HtmlNode jdNode = doc.SelectSingleNode("//div[@class='content right']");

            HtmlNode titleNode = jdNode.SelectSingleNode("h3");

            HtmlNode nodeToRemove = jdNode.FirstChild;
            while (nodeToRemove.Name != "hr")
            {
                jdNode.RemoveChild(nodeToRemove);
                nodeToRemove = jdNode.FirstChild;
            }
            jdNode.RemoveChild(nodeToRemove);

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = "Seattle, WA",
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(jdNode),
                FullHtmlDescription = jdNode.InnerHtml
            };
        }
    }
}