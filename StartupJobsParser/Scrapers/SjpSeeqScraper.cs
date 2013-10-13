using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace StartupJobsParser
{
    public class SjpSeeqScraper : SjpScraper
    {
        private Uri _defaultUri = new Uri("https://seeq.recruiterbox.com/");
        public override string CompanyName { get { return "Seeq"; } }
        public override Uri DefaultUri
        {
            get { return _defaultUri; }
        }

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