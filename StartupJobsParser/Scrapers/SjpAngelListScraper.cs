using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace StartupJobsParser
{
    public class SjpAngelListScraper : SjpScraper
    {
        private Uri _defaultUri;
        private string _companyName;

        public override string CompanyName { get { return _companyName; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }
        public override Uri PublicUri { get { return _defaultUri; } }

        public SjpAngelListScraper(
            SjpScraperParams scraperParams,
            string companyName,
            string uri
            )
            : this(scraperParams, companyName, new Uri(uri))
        {
        }

        public SjpAngelListScraper(
            SjpScraperParams scraperParams,
            string companyName,
            Uri uri
            )
            : base(scraperParams)
        {
            // TODO: Refactor. Assignment of variable name here is bad since base class may try to access.
            _defaultUri = uri;
            _companyName = companyName;
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode linkNode in doc.DocumentNode.SelectNodes("//a[contains(@class,'job-link')]"))
            {
                yield return GetAngelListJd(new Uri(uri, linkNode.Attributes["href"].Value));
            }
        }

        private JobDescription GetAngelListJd(Uri jdUri)
        {
            HtmlNode jdNode = SjpUtils.GetHtmlDoc(jdUri).DocumentNode;

            HtmlNode headerNode = jdNode.SelectSingleNode("//h1[contains(@class,'join-title')]");
            string headerText = SjpUtils.GetCleanTextFromHtml(headerNode);
            int lastAt = headerText.LastIndexOf(" at ");
            if (lastAt < 0)
            {
                throw new Exception("Couldn't find expected separator");
            }
            string title = headerText.Substring(0, lastAt);

            HtmlNode locationNode = jdNode.SelectSingleNode("//div[@class='locations']");
            HtmlNode descriptionNode = jdNode.SelectSingleNode("//div[contains(@class,'about_container')]");

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = title,
                Location = SjpUtils.GetCleanTextFromHtml(locationNode),
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}