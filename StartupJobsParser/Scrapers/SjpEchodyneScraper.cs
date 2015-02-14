using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace StartupJobsParser
{
    public class SjpEchodyneScraper : SjpScraper
    {
        private static readonly Uri _defaultUri = new Uri("http://echodyne.com/careers/");
        private string _defaultLocation = "Seattle, WA";

        public override string CompanyName { get { return "Echodyne"; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }
        public override Uri PublicUri { get { return _defaultUri; } }

        public SjpEchodyneScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//a[@title and contains(@href,'careers/')]");
            if (nodes != null && nodes.Count > 0)
            {
                foreach (HtmlNode jdLink in nodes)
                {
                    Uri jdUri = new Uri(uri, jdLink.Attributes["href"].Value);
                    yield return GetBizibleJd(jdUri);
                }
            }
        }

        private JobDescription GetBizibleJd(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);

            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//*[@class='entry-title']");
            HtmlNode descriptionNode = doc.DocumentNode.SelectSingleNode("//div[@class='entry-content']/div[@class='row']/div");
            while (!descriptionNode.InnerText.Contains("Job Description"))
            {
                descriptionNode = descriptionNode.NextSibling;
            }

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = SjpUtils.GetCleanTextFromHtml(_defaultLocation),
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}