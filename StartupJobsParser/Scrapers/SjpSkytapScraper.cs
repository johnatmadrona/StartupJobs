using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpSkytapScraper : SjpScraper
    {
        private Uri _defaultUri = new Uri("http://www.skytap.com/company/careers/?/about-us/careers/index.php");
        public override string CompanyName { get { return "Skytap"; } }
        public override Uri DefaultUri
        {
            get { return _defaultUri; }
        }

        public SjpSkytapScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//a[starts-with(@href, 'http://www.skytap.com/company/careers/')]"))
            {
                yield return GetSkytapJd(new Uri(jdLink.Attributes["href"].Value));
            }
        }

        private JobDescription GetSkytapJd(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            HtmlNode jdNode = doc.DocumentNode.SelectSingleNode("//div[@id='content']/div/div[@class='column']");

            HtmlNode titleNode = jdNode.SelectSingleNode("h1");

            titleNode.ParentNode.RemoveChild(titleNode, false);

            return new JobDescription()
            {
                SourceUri = uri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = "Seattle, WA",
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(jdNode),
                FullHtmlDescription = jdNode.InnerHtml
            };
        }
    }
}