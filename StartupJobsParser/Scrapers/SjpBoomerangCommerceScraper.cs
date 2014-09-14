using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace StartupJobsParser
{
    public class SjpBoomerangCommerceScraper : SjpScraper
    {
        private static readonly Uri _defaultUri = new Uri("http://www.boomerangcommerce.com/careers/");

        public override string CompanyName { get { return "Boomerang Commerce"; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }
        public override Uri PublicUri { get { return _defaultUri; } }

        public SjpBoomerangCommerceScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jobNode in doc.DocumentNode.SelectNodes("//div[@class='desc-wrap']"))
            {
                HtmlNode headNode = jobNode.SelectSingleNode("div[@class='title']");
                string headText = SjpUtils.GetCleanTextFromHtml(headNode);
                const string separator = " - ";
                int separation = headText.LastIndexOf(separator);
                string title = null;
                string location = null;
                if (separation < 0)
                {
                    title = headText;
                }
                else
                {
                    title = headText.Substring(0, separation);
                    location = headText.Substring(separation + separator.Length);
                }


                HtmlNode linkNode = jobNode.SelectSingleNode("descendant::a");
                Uri linkUri = new Uri(uri, linkNode.Attributes["href"].Value);

                yield return GetBoomerangeCommerceJd(title, location, linkUri);
            }
        }

        private JobDescription GetBoomerangeCommerceJd(
            string titleFromSummary,
            string locationFromSummary,
            Uri jdUri
            )
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);
            HtmlNode jdNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'ourBlk')]");

            HtmlNode bodyNode = jdNode.SelectSingleNode("descendant::div[@class='blogBody']");
            HtmlNode bodyItemNode = bodyNode.SelectSingleNode("p");

            HtmlNode locationNode = bodyItemNode;
            const string locationPrefix = "Location:";
            while (locationNode != null && !SjpUtils.GetCleanTextFromHtml(locationNode).StartsWith(locationPrefix))
            {
                locationNode = locationNode.NextSibling;
            }
            if (locationNode != null)
            {
                bodyNode.RemoveChild(locationNode);
            }

            string location = "Sunnyvale, CA";
            if (locationFromSummary != null)
            {
                location = locationFromSummary;
            }
            else if (locationNode != null)
            {
                location = SjpUtils.GetCleanTextFromHtml(locationNode).Substring(locationPrefix.Length).Trim();
            }

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = titleFromSummary,
                Location = location,
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(bodyNode),
                FullHtmlDescription = bodyNode.InnerHtml
            };
        }
    }
}