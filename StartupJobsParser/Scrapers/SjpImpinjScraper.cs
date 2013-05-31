using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpImpinjScraper : SjpScraper
    {
        private Uri _defaultUri = new Uri("http://www.hirebridge.com/jobseeker2/Searchjobresults.asp?cid=6387");
        public override string CompanyName { get { return "Impinj"; } }
        public override Uri DefaultUri
        {
            get { return _defaultUri; }
        }

        public SjpImpinjScraper(string storageDirPath, ISjpIndex index)
            : base(storageDirPath, index)
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

            string title = jdNode.SelectSingleNode("html/body/table/tr/td[@class='InteriorHeader']").InnerText;
            
            string location = "Seattle, WA";
            foreach (HtmlNode entry in jdNode.SelectNodes("//table[@class='InteriorTable']/tr/td"))
            {
                if (entry.InnerText.Trim().StartsWith("Location:", StringComparison.OrdinalIgnoreCase))
                {
                    HtmlNode next = entry.NextSibling;
                    while (!next.OriginalName.Equals("td", StringComparison.OrdinalIgnoreCase))
                    {
                        next = next.NextSibling;
                    }
                    location = next.InnerText;
                    break;
                }
            }

            HtmlNode descriptionNode = jdNode.SelectSingleNode("//table[@class='InteriorPage']");
            foreach (HtmlNode remove in descriptionNode.SelectNodes("//*[@class='button2']"))
            {
                remove.ParentNode.RemoveChild(remove, false);
            }
            string description = descriptionNode.InnerHtml;

            return new JobDescription()
            {
                SourceUri = uri.AbsoluteUri,
                Company = CompanyName,
                Title = WebUtility.HtmlDecode(title).Trim(),
                Location = WebUtility.HtmlDecode(location).Trim(),
                FullTextDescription = WebUtility.HtmlDecode(description).Trim(),
                FullHtmlDescription = description
            };
        }
    }
}