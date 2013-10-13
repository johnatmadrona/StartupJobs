using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpBuuteeqScraper : SjpScraper
    {
        private Uri _defaultUri = new Uri("http://jobs.buuteeq.me/current-openings/current-openings.htm");
        public override string CompanyName { get { return "buuteeq"; } }
        public override Uri DefaultUri
        {
            get { return _defaultUri; }
        }

        public SjpBuuteeqScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//div[@class='artMainCon']//a[contains(@href, '/current-openings/')]"))
            {
                yield return GetBuuteeqJd(new Uri(uri, jdLink.Attributes["href"].Value));
            }
        }

        private JobDescription GetBuuteeqJd(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);
            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//div[@class='artMainCon']/h1");
            HtmlNode descriptionNode = doc.DocumentNode.SelectSingleNode("//div[@class='artMainCon']");

            return new JobDescription()
            {
                SourceUri = TryCreateTrackedLink(jdUri),
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = "Seattle, WA",
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}