using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpPlacedScraper : SjpScraper
    {
        private Uri _defaultUri = new Uri("http://www.placed.com/about/careers");
        public override string CompanyName { get { return "Placed"; } }
        public override Uri DefaultUri
        {
            get { return _defaultUri; }
        }

        public SjpPlacedScraper(string storageDirPath, ISjpIndex index)
            : base(storageDirPath, index)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//div[@class='openings']/div[@class='group']/p/a"))
            {
                yield return GetPlacedJd(new Uri(uri, jdLink.Attributes["href"].Value));
            }
        }

        private JobDescription GetPlacedJd(Uri uri)
        {
            HtmlNode jdNode = SjpUtils.GetHtmlDoc(uri).DocumentNode;

            HtmlNode titleNode = jdNode.SelectSingleNode("//h1[@class='banner_title']");
            HtmlNode descriptionNode = jdNode.SelectSingleNode("//div[@id='content']");

            return new JobDescription()
            {
                SourceUri = uri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = "Seattle, WA",
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}