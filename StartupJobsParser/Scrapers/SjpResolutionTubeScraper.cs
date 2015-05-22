using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StartupJobsParser
{
    public class SjpResolutionTubeScraper : SjpScraper
    {
        private static readonly Uri _defaultUri = new Uri("http://www.resolutiontube.com/");
        private const string _defaultLocation = "Seattle, WA";

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
            foreach (HtmlNode jobLink in doc.DocumentNode.SelectNodes("//a[starts-with(@href,'apply/')]"))
            {
                yield return GetResolutionTubeJd(new Uri(uri, jobLink.Attributes["href"].Value));
            }
        }

        private JobDescription GetResolutionTubeJd(Uri jdUri)
        {
            HtmlNode doc = SjpUtils.GetHtmlDoc(jdUri).DocumentNode;
            HtmlNode titleNode = doc.SelectSingleNode("//header//h1");

            HtmlNode descriptionNode = doc.SelectSingleNode("//section/div[@class='container']");
            HtmlNode headerNode = descriptionNode.SelectSingleNode("//header");
            descriptionNode.RemoveChild(headerNode);

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