using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StartupJobsParser
{
    public class SjpMercentScraper : SjpScraper
    {
        private static readonly Uri _defaultUri = new Uri("http://www.mercent.com/careers");
        private const string _defaultLocation = "Seattle, WA";

        public override string CompanyName { get { return "Mercent"; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }
        public override Uri PublicUri { get { return _defaultUri; } }

        public SjpMercentScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//ul[@class='career-items']/li/a");
            if (nodes != null && nodes.Count > 0)
            {
                foreach (HtmlNode jdLink in nodes)
                {
                    Uri jdUri = new Uri(uri, jdLink.Attributes["href"].Value);
                    yield return GetMercentJd(jdUri);
                }
            }
        }

        private JobDescription GetMercentJd(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);

            // There are multiple of these nodes. The first is thec one we want.
            HtmlNodeCollection contentNodes = doc.DocumentNode.SelectNodes("//div[@id='wrapper']/div/div[@class='row']");

            HtmlNode titleNode = contentNodes[0];
            HtmlNode descriptionNode = contentNodes[1].SelectSingleNode("div");

            HtmlNode currNode = descriptionNode.FirstChild;
            while (!SjpUtils.GetCleanTextFromHtml(currNode).StartsWith("How To Apply"))
            {
                currNode = currNode.NextSibling;
            }

            while (currNode != null)
            {
                HtmlNode next = currNode.NextSibling;
                descriptionNode.RemoveChild(currNode);
                currNode = next;
            }

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = _defaultLocation,
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}