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

            HtmlNodeCollection jdLinkNodes = doc.DocumentNode.SelectNodes("//a[@class='a_joblist']");
            if (jdLinkNodes != null)
            {
                foreach (HtmlNode jdLinkNode in jdLinkNodes)
                {
                    yield return GetSeeqJd(new Uri(uri, jdLinkNode.Attributes["href"].Value));
                }
            }
        }

        private JobDescription GetSeeqJd(Uri jdUri)
        {
            HtmlNode jdNode = SjpUtils.GetHtmlDoc(jdUri).DocumentNode;

            HtmlNode titleNode = jdNode.SelectSingleNode("//*[starts-with(@class,'jobtitle')]");
            HtmlNode locationNode = jdNode.SelectSingleNode("//*[@class='meta-job-location-city']");

            // RecruiterBox mispelled class name "jobdesciption". It's not a mistake here.
            // Added some handling logic since they may fix it.
            HtmlNode descriptionNode = jdNode.SelectSingleNode("//div[@class='jobdesciption']");
            if (descriptionNode == null)
            {
                descriptionNode = jdNode.SelectSingleNode("//div[@class='jobdescription']");
            }

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = SjpUtils.GetCleanTextFromHtml(locationNode),
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}