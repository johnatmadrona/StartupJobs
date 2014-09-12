using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StartupJobsParser
{
    public class SjpMaxPointScraper : SjpScraper
    {
        private static readonly Uri _defaultUri = new Uri("http://maxpoint.com/us/digital-advertising-company/online-advertising-careers/online-advertising-jobs");
        private static readonly Uri _defaultScrapeUri = new Uri("http://newton.newtonsoftware.com/career/CareerHome.action?clientId=8afc05ca36a0fff80136a2d219b93475");

        public override string CompanyName { get { return "MaxPoint"; } }
        public override Uri DefaultScrapeUri { get { return _defaultScrapeUri; } }
        public override Uri PublicUri { get { return _defaultUri; } }

        public SjpMaxPointScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//div[@class='gnewtonCareerGroupJobTitleClass']/a"))
            {
                yield return GetMaxPointJd(
                    SjpUtils.GetCleanTextFromHtml(jdLink),
                    new Uri(uri, jdLink.Attributes["href"].Value)
                    );
            }
        }

        private JobDescription GetMaxPointJd(string title, Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);

            string location = "Morrisville, NC";
            HtmlNode locationNode = doc.DocumentNode.SelectSingleNode("//td[@id='gnewtonJobLocation']");
            if (locationNode != null)
            {
                string locText = SjpUtils.GetCleanTextFromHtml(locationNode);
                Regex ex = new Regex(@"Location:(?<loc>.*)");
                Match m = ex.Match(locText);
                if (m.Success)
                {
                    location = m.Groups["loc"].Value.Trim();
                }
            }

            HtmlNode descriptionNode = doc.DocumentNode.SelectSingleNode("//td[@id='gnewtonJobDescriptionText']");

            // First div in description is *usually* title, but this isn't 
            // consistent as some descriptions leave it off. We check the 
            // length as a rough heuristic.
            HtmlNode titleNodeInDesc = descriptionNode.SelectSingleNode("div");
            if (SjpUtils.GetCleanTextFromHtml(titleNodeInDesc).Length < 75)
            {
                titleNodeInDesc.Remove();
            }

            return new JobDescription()
            {
                SourceUri = jdUri.AbsolutePath,
                Company = CompanyName,
                Title = title,
                Location = location,
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}