using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpRedfinScraper : SjpJobviteScraper
    {
        private const string _redfinCompanyId = "qf49Vfw7";
        private static string _scraperUrl = string.Format(_listUriFormat + "&page=Jobs-Location", _redfinCompanyId);

        public SjpRedfinScraper(SjpScraperParams scraperParams)
            : base(scraperParams, "Redfin", "http://www.redfin.com/about/jobs", _redfinCompanyId, _scraperUrl)
        {
        }

        protected override JobDescription GetJd(Uri jdUri)
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