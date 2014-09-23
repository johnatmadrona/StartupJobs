using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StartupJobsParser
{
    public class SjpResolutionTubeScraper : SjpScraper
    {
        private static readonly Uri _defaultUri = new Uri("http://www.resolutiontube.com/apply/");

        public override string CompanyName { get { return "ResolutionTube"; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }
        public override Uri PublicUri { get { return _defaultUri; } }

        public SjpResolutionTubeScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jobNode in doc.DocumentNode.SelectNodes("//div[@class='job']"))
            {
                HtmlNode titleNode = jobNode.SelectSingleNode("div[@class='job_title']");
                HtmlNode locationNode = jobNode.SelectSingleNode("div[@class='job_subtitle']");
                HtmlNode linkNode = jobNode.SelectSingleNode("div[@class='job_title']/a");
                yield return GetResolutionTubeJd(
                    titleNode,
                    locationNode,
                    new Uri(uri, linkNode.Attributes["href"].Value)
                    );
            }
        }

        private JobDescription GetResolutionTubeJd(HtmlNode titleNode, HtmlNode locationNode, Uri jdUri)
        {
            HtmlNode doc = SjpUtils.GetHtmlDoc(jdUri).DocumentNode;
            HtmlNode descriptionNode = doc.SelectSingleNode("//body/div[contains(@class, 'job_posting_section')]");

            return new JobDescription()
            {
                SourceUri = PublicUri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = SjpUtils.GetCleanTextFromHtml(locationNode),
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}