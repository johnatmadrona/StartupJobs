using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpIntrepidLearningScraper : SjpJobviteScraper
    {
        public SjpIntrepidLearningScraper(SjpScraperParams scraperParams)
            : base(scraperParams, "Intrepid Learning Solutions", "http://intrepidlearning.com/careers/", "q089VfwW")
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdUriNode in doc.DocumentNode.SelectNodes("//a[@class='jvjoblink']"))
            {
                yield return GetIntrepidLearningJd(ExtractJdUriFromGoToPageLink(jdUriNode));
            }
        }

        private JobDescription GetIntrepidLearningJd(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);

            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//div[@class='jvheader']");

            HtmlNode locationNode = titleNode.NextSibling;
            string location = null;
            while (locationNode != null && location == null)
            {
                string locationText = SjpUtils.GetCleanTextFromHtml(locationNode);
                if (locationText.Contains("|"))
                {
                    string[] tokens = locationText.Split('|');
                    location = tokens[tokens.Length - 1].Trim();
                }
                else
                {
                    locationNode = locationNode.NextSibling;
                }
            }

            HtmlNode descriptionNode = titleNode.ParentNode;
            foreach (HtmlNode remove in descriptionNode.SelectNodes("div"))
            {
                remove.ParentNode.RemoveChild(remove, false);
            }
            foreach (HtmlNode remove in descriptionNode.SelectNodes("script"))
            {
                remove.ParentNode.RemoveChild(remove, false);
            }

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