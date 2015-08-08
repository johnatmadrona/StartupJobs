using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StartupJobsParser
{
    public class SjpJobalineScraper : SjpScraper
    {
        private static readonly Uri _defaultUri = new Uri("http://jobalineinc.jobaline.com/Search");
        private string _defaultLocation = "Kirkland, WA";

        public override string CompanyName { get { return "Jobaline"; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }
        public override Uri PublicUri { get { return _defaultUri; } }

        public SjpJobalineScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//a[@class='search_job_title']");
            foreach (HtmlNode jdLink in nodes)
            {
                Uri jdUri = new Uri(uri, jdLink.Attributes["href"].Value);
                yield return GetJobalineJd(jdUri);
            }
        }

        private JobDescription GetJobalineJd(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);
            HtmlNode jdNode = doc.DocumentNode;

            HtmlNode titleNode = jdNode.SelectSingleNode("//*[contains(@class,'job-title')]");

            HtmlNode locationNode = jdNode.SelectSingleNode("//span[@class='info-icons']");
            Regex locationEx = new Regex("(?<location>[a-zA-Z ]+, [A-Z]{2})");
            Match m = locationEx.Match(locationNode.InnerText);
            string location = _defaultLocation;
            if (m.Success)
            {
                location = m.Groups["location"].Value.Trim();
            }

            HtmlNode descriptionNode = jdNode.SelectSingleNode("//div[@class='job-description-container']");

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