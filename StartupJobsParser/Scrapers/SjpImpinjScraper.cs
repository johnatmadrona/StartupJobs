using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpImpinjScraper : SjpScraper
    {
        private static readonly Uri _defaultScrapeUri = new Uri("http://www.hirebridge.com/jobseeker2/Searchjobresults.asp?cid=6387");
        private static readonly Uri _publicUri = new Uri("http://www.impinj.com/careers/");

        public override string CompanyName { get { return "Impinj"; } }
        public override Uri DefaultScrapeUri { get { return _defaultScrapeUri; } }
        public override Uri PublicUri { get { return _publicUri; } }

        public SjpImpinjScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//td[@class='SearchResultRow']/a"))
            {
                yield return GetImpinjJd(new Uri(uri, jdLink.Attributes["href"].Value));
            }
        }

        private JobDescription GetImpinjJd(Uri uri)
        {
            HtmlNode jdNode = SjpUtils.GetHtmlDoc(uri).DocumentNode;

            HtmlNode titleNode = jdNode.SelectSingleNode("html/body/table/tr/td[@class='InteriorHeader']");
            
            HtmlNode locationNode = null;
            foreach (HtmlNode entry in jdNode.SelectNodes("//table[@class='InteriorTable']/tr/td"))
            {
                if (entry.InnerText.Trim().StartsWith("Location:", StringComparison.OrdinalIgnoreCase))
                {
                    HtmlNode next = entry.NextSibling;
                    while (!next.OriginalName.Equals("td", StringComparison.OrdinalIgnoreCase))
                    {
                        next = next.NextSibling;
                    }
                    locationNode = next;
                    break;
                }
            }

            HtmlNode descriptionNode = jdNode.SelectSingleNode("//table[@class='InteriorPage']");
            foreach (HtmlNode remove in descriptionNode.SelectNodes("//*[@class='button2']"))
            {
                remove.ParentNode.RemoveChild(remove, false);
            }

            return new JobDescription()
            {
                SourceUri = TryCreateTrackedLink(PublicTaggedUri),
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = SjpUtils.GetCleanTextFromHtml(locationNode),
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}