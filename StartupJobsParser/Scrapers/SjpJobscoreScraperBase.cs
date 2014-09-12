using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace StartupJobsParser
{
    public abstract class SjpJobscoreScraperBase : SjpScraper
    {
        protected SjpJobscoreScraperBase(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//td/a[starts-with(@href, 'http://www.jobscore.com/jobs2/')]"))
            {
                HtmlNode locationNode = jdLink.ParentNode.NextSibling;
                while (locationNode.Name != "td")
                {
                    locationNode = locationNode.NextSibling;
                }
                yield return GetJobscoreJd(
                    SjpUtils.GetCleanTextFromHtml(jdLink),
                    SjpUtils.GetCleanTextFromHtml(locationNode),
                    new Uri(jdLink.Attributes["href"].Value)
                    );
            }
        }

        protected JobDescription GetJobscoreJd(string jobTitle, string jobLocation, Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);
            HtmlNode descriptionNode = doc.DocumentNode.SelectSingleNode("//div[@class='js-job-description']");

            return new JobDescription()
            {
                SourceUri = jdUri.AbsolutePath,
                Company = CompanyName,
                Title = jobTitle,
                Location = jobLocation,
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}