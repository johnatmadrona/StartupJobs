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
            HtmlNode jdNode = doc.DocumentNode.SelectSingleNode("//div[@id='jobPost']");

            HtmlNode titleNode = jdNode.SelectSingleNode("//div[@id='jaJobTitle']");
            HtmlNode locationNode = jdNode.SelectSingleNode("//div[@id='jaLocation']");
            HtmlNode descriptionNode = jdNode.SelectSingleNode("//div[@id='jobDescription']");

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