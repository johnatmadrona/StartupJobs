using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpHaikuDeckScraper : SjpScraper
    {
        private static readonly Uri _defaultUri = new Uri("http://www.haikudeck.com/jobs");

        public override string CompanyName { get { return "Haiku Deck"; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }
        public override Uri PublicUri { get { return _defaultUri; } }

        public SjpHaikuDeckScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdNode in doc.DocumentNode.SelectNodes("//div[@class='accordion']/div[@class='accordion-group']"))
            {
                yield return GetHaikuDeckJd(jdNode, uri);
            }
        }

        private JobDescription GetHaikuDeckJd(HtmlNode jdNode, Uri uri)
        {
            HtmlNode titleNode = jdNode.SelectSingleNode("div[@class='accordion-heading']");
            HtmlNode descriptionNode = jdNode.SelectSingleNode("div/div[@class='accordion-inner']");

            return new JobDescription()
            {
                SourceUri = TryCreateTrackedLink(PublicTaggedUri),
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = "Fremont, WA",
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}