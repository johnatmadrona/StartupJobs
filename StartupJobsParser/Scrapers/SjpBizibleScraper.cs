using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StartupJobsParser
{
    public class SjpBizibleScraper : SjpScraper
    {
        private static readonly Uri _defaultUri = new Uri("http://www.bizible.com/jobs/");
        private string _defaultLocation = "Seattle, WA";

        public override string CompanyName { get { return "Bizible"; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }
        public override Uri PublicUri { get { return _defaultUri; } }

        public SjpBizibleScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//a[contains(@title, 'job description') and starts-with(@href,'/jobs/')]"))
            {
                Uri jdUri = new Uri(uri, jdLink.Attributes["href"].Value);
                yield return GetBizibleJd(jdUri);
            }
        }

        private JobDescription GetBizibleJd(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);

            HtmlNode descriptionNode = doc.DocumentNode.SelectSingleNode("//div[@class='ISContent Normal']");

            HtmlNode titleNode = descriptionNode.SelectSingleNode("h1");

            HtmlNode nodeToRemove = null;
            Regex locationRegex = new Regex(@"(?<Location>\w+, [A-Z]{2})[^\w]");
            string location = _defaultLocation;
            foreach (HtmlNode node in descriptionNode.SelectNodes("p/strong"))
            {
                Match m = locationRegex.Match(node.InnerText);
                if (m.Success)
                {
                    location = m.Groups["Location"].Value;
                    nodeToRemove = node.ParentNode;
                    break;
                }
            }

            while (nodeToRemove != null)
            {
                HtmlNode nextToRemove = nodeToRemove.PreviousSibling;
                descriptionNode.RemoveChild(nodeToRemove);
                nodeToRemove = nextToRemove;
            }

            return new JobDescription()
            {
                SourceUri = TryCreateTrackedLink(jdUri),
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = SjpUtils.GetCleanTextFromHtmlEncodedText(location),
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}