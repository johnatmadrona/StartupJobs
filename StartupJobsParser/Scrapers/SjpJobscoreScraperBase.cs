using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace StartupJobsParser
{
    public abstract class SjpJobscoreScraperBase : SjpScraper
    {
        public abstract Uri PublicUri { get; }

        protected SjpJobscoreScraperBase(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdListing in doc.DocumentNode.SelectNodes("//tr[@class='job-listing']"))
            {
                HtmlNode titleAndLink = jdListing.SelectSingleNode("td[@class='job-title']/a");
                HtmlNode location = jdListing.SelectSingleNode("td[@class='job-attribute']");
                yield return GetJobscoreJd(
                    SjpUtils.GetCleanTextFromHtml(titleAndLink),
                    SjpUtils.GetCleanTextFromHtml(location),
                    new Uri(titleAndLink.Attributes["href"].Value)
                    );
            }
        }

        protected JobDescription GetJobscoreJd(string jobTitle, string jobLocation, Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);
            HtmlNode descriptionNode =
                doc.DocumentNode.SelectSingleNode(
                    "//div[@class='main_container']/div[starts-with(@class, 'left')]"
                    );

            // .NET only supports XPath 1.0, so 'ends-with' is not available
            // XPath substring operation starts with index of 1 instead 0
            HtmlNode remove = descriptionNode.SelectSingleNode("div[substring(@class, string-length(@class) - 3)='_top']");
            descriptionNode.RemoveChild(remove);
                
            remove = descriptionNode.SelectSingleNode("div[@class='dont_print']");
            descriptionNode.RemoveChild(remove);

            return new JobDescription()
            {
                SourceUri = TryCreateTrackedLink(jdUri),
                Company = CompanyName,
                Title = jobTitle,
                Location = jobLocation,
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}