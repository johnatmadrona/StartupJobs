using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StartupJobsParser
{
    public class SjpResolutionTubeScraper : SjpScraper
    {
        private static readonly Uri _defaultUri = new Uri("http://www.resolutiontube.com/apply/");
        private string _defaultLocation = "Seattle, WA";

        public override string CompanyName { get { return "Resolution Tube"; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }
        public override Uri PublicUri { get { return _defaultUri; } }

        public SjpResolutionTubeScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[starts-with(@id, 'job')]/div");
            if (nodes != null && nodes.Count > 0)
            {
                foreach (HtmlNode jobNode in nodes)
                {
                    yield return GetResolutionTubeJd(jobNode);
                }
            }
        }

        private JobDescription GetResolutionTubeJd(HtmlNode jobNode)
        {
            HtmlNode titleNode = jobNode.SelectSingleNode("h3");
            titleNode.Remove();

            HtmlNode applyNode = jobNode.SelectSingleNode("p/strong[starts-with(text(), 'To apply')]");
            applyNode.Remove();

            return new JobDescription()
            {
                SourceUri = TryCreateTrackedLink(PublicTaggedUri),
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = _defaultLocation,
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(jobNode),
                FullHtmlDescription = jobNode.InnerHtml
            };
        }
    }
}