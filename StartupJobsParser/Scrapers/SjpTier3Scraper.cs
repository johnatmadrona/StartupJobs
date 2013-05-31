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

        public SjpTier3Scraper(string storageDirPath, ISjpIndex index)
            : base(storageDirPath, index)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//article[contains(@class,'job-item')]/a"))
            {
                string location = jdLink.SelectSingleNode("span[@class='location']").InnerText;
                yield return GetTier3Jd(new Uri(uri, jdLink.Attributes["href"].Value), location);
            }
        }

        private JobDescription GetTier3Jd(Uri uri, string location)
        {
            HtmlNode jdNode = SjpUtils.GetHtmlDoc(uri).DocumentNode;

            HtmlNode targetNode = jdNode.SelectSingleNode("//div[@class='container']/div[@class='row']");
            string title = targetNode.SelectSingleNode("div[@class='span12']/h1").InnerText;
            string description = targetNode.SelectSingleNode("div[@class='span9']").InnerHtml;

            return new JobDescription()
            {
                SourceUri = uri.AbsoluteUri,
                Company = CompanyName,
                Title = WebUtility.HtmlDecode(title).Trim(),
                Location = WebUtility.HtmlDecode(location).Trim(),
                FullTextDescription = WebUtility.HtmlDecode(description).Trim(),
                FullHtmlDescription = description
            };
        }
    }
}