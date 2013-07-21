using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpTier3Scraper : SjpScraper
    {
        private Uri _defaultUri = new Uri("http://www.tier3.com/careers");
        public override string CompanyName { get { return "Tier 3"; } }
        public override Uri DefaultUri
        {
            get { return _defaultUri; }
        }

        public SjpTier3Scraper(ISjpStorage storage, ISjpIndex index)
            : base(storage, index)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//article[contains(@class,'job-item')]/a"))
            {
                string location = SjpUtils.GetCleanTextFromHtml(
                    jdLink.SelectSingleNode("span[@class='location']")
                    );
                yield return GetTier3Jd(new Uri(uri, jdLink.Attributes["href"].Value), location);
            }
        }

        private JobDescription GetTier3Jd(Uri uri, string location)
        {
            HtmlNode jdNode = SjpUtils.GetHtmlDoc(uri).DocumentNode;

            HtmlNode targetNode = jdNode.SelectSingleNode("//div[@class='container']/div[@class='row']");
            HtmlNode titleNode = targetNode.SelectSingleNode("div[@class='span12']/h1");
            HtmlNode descriptionNode = targetNode.SelectSingleNode("div[@class='span9']");

            return new JobDescription()
            {
                SourceUri = uri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = location,
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}