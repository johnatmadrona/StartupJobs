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
            foreach (HtmlNode geoLinkNode in doc.DocumentNode.SelectNodes("//a[contains(@href,'-office.htm')]"))
            {
                Uri geoLinkUri = new Uri(uri, geoLinkNode.Attributes["href"].Value);
                string geoLinkPath = geoLinkUri.AbsolutePath;

                HtmlDocument geoDoc = SjpUtils.GetHtmlDoc(geoLinkUri);

                // Note: The html is bad at these pages. E.g. Has <li><li>...</li></li> instead of <li></li><li></li>
                foreach (HtmlNode linkNode in geoDoc.DocumentNode.SelectNodes("//div[@id='sideBar']/ul[@class='middcont']//a"))
                {
                    string href = linkNode.Attributes["href"].Value;
                    if (href.Length > geoLinkPath.Length && href.Substring(href.Length - geoLinkPath.Length) != geoLinkPath)
                    {
                        yield return GetBuuteeqJd(new Uri(uri, href));
                    }
                }
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