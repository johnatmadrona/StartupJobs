using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace StartupJobsParser
{
    public class SjpSeeqScraper : SjpScraper
    {
        private static readonly Uri _defaultScrapeUri = new Uri("https://seeq.recruiterbox.com/");
        private static readonly Uri _publicUri = new Uri("https://seeq.com/index.php/company/careers");

        public override string CompanyName { get { return "Seeq"; } }
        public override Uri DefaultScrapeUri { get { return _defaultScrapeUri; } }
        public override Uri PublicUri { get { return _publicUri; } }

        public SjpSeeqScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);

            HtmlNodeCollection jdInfoNodes = doc.DocumentNode.SelectNodes("//div[@class='div_job_item_name']/a");
            if (jdInfoNodes != null)
            {
                foreach (HtmlNode jdInfoNode in jdInfoNodes)
                {
                    yield return GetSeeqJd(new Uri(uri, jdInfoNode.Attributes["href"].Value));
                }
            }
        }

        private JobDescription GetSeeqJd(Uri uri)
        {
            HtmlNode jdNode = SjpUtils.GetHtmlDoc(uri).DocumentNode;

            HtmlNode titleNode = jdNode.SelectSingleNode("//h1[@id='h1_opening_name']");
            HtmlNode locationNode = jdNode.SelectSingleNode("//h2[@id='h2_job_info']");

            HtmlNode descriptionNode = jdNode.SelectSingleNode("//div[@id='div_jd']");

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