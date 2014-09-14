using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpPeachScraper : SjpScraper
    {
        private static readonly Uri _defaultUri = new Uri("https://www.peachd.com/jobs");

        public override string CompanyName { get { return "Peach"; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }
        public override Uri PublicUri { get { return _defaultUri; } }

        public SjpPeachScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode headerNode in doc.DocumentNode.SelectNodes("//div[starts-with(@onclick,'toggle(')]"))
            {
                HtmlNode bodyNode = headerNode.NextSibling;
                while (bodyNode != null && 
                    (!bodyNode.Attributes.Contains("class") || bodyNode.Attributes["class"].Value != "jobsSection"))
                {
                    bodyNode = bodyNode.NextSibling;
                }

                yield return GetPeachJd(headerNode, bodyNode);
            }
        }

        private JobDescription GetPeachJd(HtmlNode headerNode, HtmlNode bodyNode)
        {
            HtmlNodeCollection headerItems = headerNode.SelectNodes("div");
            HtmlNode titleNode = headerItems[0];
            HtmlNode locationNode = headerItems[1];

            return new JobDescription()
            {
                SourceUri = PublicUri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = SjpUtils.GetCleanTextFromHtml(locationNode),
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(bodyNode),
                FullHtmlDescription = bodyNode.InnerHtml
            };
        }
    }
}