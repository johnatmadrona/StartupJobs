using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpMixpoScraper : SjpScraper
    {
        private static readonly Uri _defaultUri = new Uri("http://dynamicvideoad.mixpo.com/about/careers/");

        public override string CompanyName { get { return "Mixpo"; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }
        public override Uri PublicUri { get { return _defaultUri; } }

        public SjpMixpoScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//a[starts-with(@href, '/about/career_position/')]"))
            {
                yield return GetMixpoJd(new Uri(uri, jdLink.Attributes["href"].Value));
            }
        }

        private JobDescription GetMixpoJd(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);
            HtmlNode jdNode = doc.DocumentNode.SelectSingleNode("//div[@id='content']");
            
            HtmlNode titleNode = jdNode.SelectSingleNode("h2");

            HtmlNode remove = jdNode.SelectSingleNode("p[@id='breadcrumb']");
            remove.ParentNode.RemoveChild(remove, false);
            remove = jdNode.SelectSingleNode("h2");
            remove.ParentNode.RemoveChild(remove, false);
            remove = jdNode.SelectSingleNode("a[starts-with(@class, 'email')]");
            remove.ParentNode.RemoveChild(remove, false);

            return new JobDescription()
            {
                SourceUri = TryCreateTrackedLink(jdUri),
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = "Seattle, WA",
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(jdNode),
                FullHtmlDescription = jdNode.InnerHtml
            };
        }
    }
}