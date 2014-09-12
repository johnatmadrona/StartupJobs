using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StartupJobsParser
{
    public class SjpMercentScraper : SjpScraper
    {
        private static readonly Uri _defaultUri = new Uri("http://www.mercent.com/careers");
        private string _defaultLocation = "Seattle, WA";

        public override string CompanyName { get { return "Mercent"; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }
        public override Uri PublicUri { get { return _defaultUri; } }

        public SjpMercentScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//ul[@class='career-items']/li/a");
            if (nodes != null && nodes.Count > 0)
            {
                foreach (HtmlNode jdLink in nodes)
                {
                    Uri jdUri = new Uri(uri, jdLink.Attributes["href"].Value);
                    yield return GetMercentJd(jdUri);
                }
            }
        }

        private JobDescription GetMercentJd(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);

            // There are multiple of these nodes. The first is the one we want.
            HtmlNode contentNode = doc.DocumentNode.SelectSingleNode("//div[@class='wrapper-content']/div[@class='container_12']");

            HtmlNode titleNode = contentNode.SelectSingleNode("h2");
            HtmlNode descriptionNode = contentNode.SelectSingleNode("div");

            // Remove functional group title
            descriptionNode.SelectSingleNode("h3").Remove();

            string location = _defaultLocation;
            HtmlNode locationStartNode = descriptionNode.SelectSingleNode(".//h4[contains(text(), 'Primary Location')]");
            if (locationStartNode != null)
            {
                HtmlNode locationNode = locationStartNode.NextSibling;
                while (locationNode.Name != "p")
                {
                    HtmlNode next = locationNode.NextSibling;
                    locationNode.Remove();
                    locationNode = next;
                }
                location = SjpUtils.GetCleanTextFromHtml(locationNode);
                Regex exp = new Regex(".*-(?<state>.*?)-(?<city>.*?)$");
                Match m = exp.Match(location);
                if (m.Success)
                {
                    location = 
                        m.Groups["city"].Value.Trim() + 
                        ", " + 
                        m.Groups["state"].Value.Trim();
                }

                locationNode.Remove();
                locationStartNode.Remove();
            }
            
            HtmlNode removalStartNode = removalStartNode = descriptionNode.SelectSingleNode("h4[text()='How To Apply']");
            while (removalStartNode != null)
            {
                HtmlNode next = removalStartNode.NextSibling;
                removalStartNode.Remove();
                removalStartNode = next;
            }

            return new JobDescription()
            {
                SourceUri = jdUri.AbsolutePath,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = location,
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}