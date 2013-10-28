using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace StartupJobsParser
{
    public class SjpQumuloScraper : SjpScraper
    {
        private static readonly Uri _defaultScrapeUri = new Uri("http://qumulo.catsone.com/careers/");
        private static readonly Uri _publicUri = new Uri("http://www.qumulo.com/#/qumulo_jobs");

        public override string CompanyName { get { return "Qumulo"; } }
        public override Uri DefaultScrapeUri { get { return _defaultScrapeUri; } }
        public override Uri PublicUri { get { return _publicUri; } }

        public SjpQumuloScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdNode in doc.DocumentNode.SelectNodes("//a[@class='jobTitle']"))
            {
                yield return GetQumuloJd(new Uri(jdNode.Attributes["href"].Value));
            }
        }

        private JobDescription GetQumuloJd(Uri uri)
        {
            HtmlNode jdNode = SjpUtils.GetHtmlDoc(uri).DocumentNode;
            HtmlNode titleNode = jdNode.SelectSingleNode("//h1[@id='jobTitle']");
            HtmlNode locationNode = jdNode.SelectSingleNode("//div[@id='jobDetailLocation']/strong");
            HtmlNode descriptionNode = jdNode.SelectSingleNode("//div[@class='detailsJobDescription']");

            return new JobDescription()
            {
                SourceUri = TryCreateTrackedLink(uri),
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = SjpUtils.GetCleanTextFromHtml(locationNode),
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}