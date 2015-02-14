using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpSmartsheetScraper : SjpJobviteScraper
    {
        public SjpSmartsheetScraper(SjpScraperParams scraperParams)
            : base(scraperParams, "Smartsheet", "http://www.smartsheet.com/careers/", "q6R9VfwL")
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdUriNode in doc.DocumentNode.SelectNodes("//ul[@class='joblist']/li/a"))
            {
                yield return GetSmartsheetJd(ExtractJdUriFromGoToPageLink(jdUriNode));
            }
        }

        private JobDescription GetSmartsheetJd(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);

            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//div[@class='jvjobheader']/h2");

            HtmlNode locationNode = doc.DocumentNode.SelectSingleNode("//div[@class='jvjobheader']/h3");
            string location = SjpUtils.GetCleanTextFromHtml(locationNode);
            if (location.Contains("|"))
            {
                location = location.Split('|')[1].Trim();
            }

            HtmlNode descriptionNode = doc.DocumentNode.SelectSingleNode("//div[@class='jvdescriptionbody']");

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = WebUtility.HtmlDecode(location).Trim(),
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}