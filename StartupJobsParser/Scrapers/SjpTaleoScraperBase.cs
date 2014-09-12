using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace StartupJobsParser
{
    public abstract class SjpTaleoScraperBase : SjpScraper
    {
        protected abstract string JdContentTableXPath { get; }

        public SjpTaleoScraperBase(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//a[contains(@href, 'requisition.jsp')]"))
            {
                yield return GetTaleoJd(new Uri(jdLink.Attributes["href"].Value));
            }
        }

        private JobDescription GetTaleoJd(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);
            HtmlNode jdNode = doc.DocumentNode.SelectSingleNode(JdContentTableXPath);

            HtmlNode titleNode = jdNode.SelectSingleNode("tr/td/h1");
            HtmlNode locationNode = jdNode.SelectSingleNode("tr/td/b");

            // TODO: We're losing some data like the wrapping table...
            HtmlNode descriptionNode = jdNode.SelectSingleNode("tr");
            while (!SjpUtils.GetCleanTextFromHtml(descriptionNode).StartsWith("Description", StringComparison.OrdinalIgnoreCase))
            {
                descriptionNode = descriptionNode.NextSibling;
            }

            return new JobDescription()
            {
                SourceUri = jdUri.AbsolutePath,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = SjpUtils.GetCleanTextFromHtml(locationNode),
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}