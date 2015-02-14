using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpRedfinScraper : SjpJobviteScraper
    {
        public SjpRedfinScraper(SjpScraperParams scraperParams)
            : base(scraperParams, "Redfin", "http://www.redfin.com/about/jobs", "qf49Vfw7")
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdUriNode in doc.DocumentNode.SelectNodes("//a[contains(@href, '/open-jobs?')]"))
            {
                // Get the jobvite job ID from the link
                Uri tmpUri = new Uri(jdUriNode.Attributes["href"].Value);
                string jobId = tmpUri.Query.Split('=')[1];
                yield return GetRedfinJd(GetItemUri(WebUtility.UrlEncode(jobId)));
            }
        }

        private JobDescription GetRedfinJd(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);

            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//h1");

            HtmlNode locationNode = doc.DocumentNode.SelectSingleNode("//h2");
            string location = SjpUtils.GetCleanTextFromHtml(locationNode);
            if (location.Contains("|"))
            {
                char[] sep = { '|' };
                location = location.Split(sep)[1].Trim();
            }

            HtmlNode descriptionNode = doc.DocumentNode.SelectSingleNode("//div[@class='jobDesc']");

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = location,
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}