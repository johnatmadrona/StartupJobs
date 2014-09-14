using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace StartupJobsParser
{
    public abstract class SjpAngelListScraperBase : SjpScraper
    {
        protected SjpAngelListScraperBase(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
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