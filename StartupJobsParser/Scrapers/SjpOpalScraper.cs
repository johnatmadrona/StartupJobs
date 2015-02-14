using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StartupJobsParser
{
    public class SjpOpalScraper : SjpScraper
    {
        private static readonly Uri _defaultUri = new Uri("http://workwithopal.com/careers");

        public override string CompanyName { get { return "Opal"; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }
        public override Uri PublicUri { get { return _defaultUri; } }

        public SjpOpalScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'position')]/a[@class='title']");
            if (nodes != null && nodes.Count > 0)
            {
                foreach (HtmlNode jdLink in nodes)
                {
                    yield return GetOpalJd(new Uri(uri, jdLink.Attributes["href"].Value));
                }
            }
        }

        private JobDescription GetOpalJd(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);

            HtmlNode descriptionNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'position_description')]");

            HtmlNode titleNode = descriptionNode.SelectSingleNode("h1");
            string title = SjpUtils.GetCleanTextFromHtml(titleNode);
            int offset = title.IndexOf("//");
            if (offset >= 0)
            {
                title = title.Substring(offset + 2).Trim();
            }
            descriptionNode.RemoveChild(titleNode);

            HtmlNode locationNode = descriptionNode.SelectSingleNode("div[@class='location']");
            descriptionNode.RemoveChild(locationNode);

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = title,
                Location = SjpUtils.GetCleanTextFromHtml(locationNode),
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}