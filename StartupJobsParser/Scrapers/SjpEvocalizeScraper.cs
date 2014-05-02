using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace StartupJobsParser
{
    public class SjpEvocalizeScraper : SjpScraper
    {
        private static readonly Uri _defaultUri = new Uri("http://evocalize.com/about/jobs");
        private string _defaultLocation = "Seattle, WA";

        public override string CompanyName { get { return "evocalize"; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }
        public override Uri PublicUri { get { return _defaultUri; } }

        public SjpEvocalizeScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            const string description = "At Evocalize, we suffer from a fierce belief that " +
                "customer opinions - and the relationship between customers and companies " +
                "- are at the nucleus of creating highly successful brands. We help " +
                "businesses cultivate deeper, stronger relationships with their customers " +
                "while giving people a powerful way to express, share, impact, and connect " + 
                "with others. As we do this, we want to work with talented, motivated, " +
                "passionate people across many disciplines who are willing to learn, grow, " +
                "create, and challenge us to achieve greater results for our clients. If " +
                "this sounds like you - and you're interested in joining a great team and " + 
                "delivering world class technology and service - we can't wait to talk to you!";

            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            HtmlNode parentNode = 
                doc.DocumentNode.SelectSingleNode("//h3[starts-with(text(), 'Positions')]").ParentNode.ParentNode;
            HtmlNodeCollection jobTitleNodes = parentNode.SelectNodes("ul/li");

            if (jobTitleNodes != null && jobTitleNodes.Count > 0)
            {
                foreach (HtmlNode tn in jobTitleNodes)
                {
                    yield return new JobDescription()
                    {
                        SourceUri = TryCreateTrackedLink(PublicTaggedUri),
                        Company = CompanyName,
                        Title = SjpUtils.GetCleanTextFromHtml(tn),
                        Location = _defaultLocation,
                        FullTextDescription = description,
                        FullHtmlDescription = description
                    };
                }
            }
        }
    }
}