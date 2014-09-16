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

        private JobDescription GetQumuloJd(Uri jdUri)
        {
            HtmlNode jdNode = SjpUtils.GetHtmlDoc(jdUri).DocumentNode;
            HtmlNode titleNode = jdNode.SelectSingleNode("//h1[@id='jobTitle']");

            HtmlNode locationNode = jdNode.SelectSingleNode("//div[@id='jobDetailLocation']/strong");
            string location = SjpUtils.GetCleanTextFromHtml(locationNode);
            int excerptStart = location.IndexOf('(');
            int excerptEnd = location.IndexOf(')');
            // Exclude parenthetical only if it's not at the beginning and if parenths are properly ordered
            if (0 < excerptStart && excerptStart < excerptEnd)
            {
                location = location.Substring(0, excerptStart).Trim() + location.Substring(excerptEnd + 1).Trim();
            }

            HtmlNode descriptionNode = jdNode.SelectSingleNode("//div[@class='detailsJobDescription']");

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = location,
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}