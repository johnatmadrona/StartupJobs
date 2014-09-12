using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpBuuteeqScraper : SjpScraper
    {
        private static readonly Uri _defaultUri = new Uri("http://jobs.buuteeq.me/current-openings/current-openings.htm");

        public override string CompanyName { get { return "buuteeq"; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }
        public override Uri PublicUri { get { return _defaultUri; } }

        public SjpBuuteeqScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//div[@id='sideBar']/ul/li[@class!='navclick']/a[contains(@href, '/current-openings/')]"))
            {
                yield return GetBuuteeqJd(new Uri(uri, jdLink.Attributes["href"].Value));
            }
        }

        private JobDescription GetBuuteeqJd(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);

            HtmlNode contentNode = doc.DocumentNode.SelectSingleNode("//div[@class='artMainCon']");
            HtmlNode titleNode = contentNode.SelectSingleNode("h1");
            HtmlNode descriptionNode = contentNode.SelectSingleNode("div");
            HtmlNode applyNode = descriptionNode.SelectSingleNode(".//strong[contains(text(), '@buuteeq.com')]");
            if (applyNode != null)
            {
                applyNode.Remove();
            }

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = "Seattle, WA",
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}