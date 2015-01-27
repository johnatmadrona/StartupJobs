using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StartupJobsParser
{
    public class SjpMaxPointScraper : SjpNewtonScraperBase
    {
        private Uri _publicUri = new Uri("http://maxpoint.com/us/digital-advertising-company/online-advertising-careers/online-advertising-jobs");
        public override string CompanyName { get { return "MaxPoint"; } }
        public override Uri PublicUri { get { return _publicUri; } }

        protected override string NewtonCompanyId { get { return "8afc05ca36a0fff80136a2d219b93475"; } }

        public SjpMaxPointScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override JobDescription GetJd(string title, Uri jdUri)
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
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = title,
                Location = location,
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}