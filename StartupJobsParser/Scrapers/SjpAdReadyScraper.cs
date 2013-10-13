using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpAdReadyScraper : SjpScraper
    {
        private Uri _defaultUri = new Uri("https://www.smartrecruiters.com/AdReadyInc");
        public override string CompanyName { get { return "AdReady"; } }
        public override Uri DefaultUri
        {
            get { return _defaultUri; }
        }

        public SjpAdReadyScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);

            HtmlNodeCollection jdInfoNodes = doc.DocumentNode.SelectNodes("//table[@class='cs_table']/tr[@class='cs_container']");
            if (jdInfoNodes != null)
            {
                foreach (HtmlNode jdInfoNode in jdInfoNodes)
                {
                    HtmlNode jdLink = jdInfoNode.SelectSingleNode("td[@class='cs_table']/a");
                    yield return GetAdReadyJd(new Uri(uri, jdLink.Attributes["href"].Value));
                }
            }
        }

        private JobDescription GetAdReadyJd(Uri uri)
        {
            HtmlNode jdNode = SjpUtils.GetHtmlDoc(uri).DocumentNode;

            HtmlNode titleNode = jdNode.SelectSingleNode("//h1[@class='jobAdTitle']");
            HtmlNode locationNode = jdNode.SelectSingleNode("//span[@class='jobAdLocation']");

            HtmlNode descriptionNode = jdNode.SelectSingleNode("//div[@class='jobAdLeft']");
            HtmlNode remove = descriptionNode.SelectSingleNode("input");
            remove.ParentNode.RemoveChild(remove, false);

            return new JobDescription()
            {
                SourceUri = uri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = SjpUtils.GetCleanTextFromHtml(locationNode),
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}