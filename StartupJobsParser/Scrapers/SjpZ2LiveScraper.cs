using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpZ2LiveScraper : SjpScraper
    {
        private Uri _defaultUri = new Uri("http://www.z2.com/careers");
        public override string CompanyName { get { return "Z2Live"; } }
        public override Uri DefaultUri
        {
            get { return _defaultUri; }
        }

        public SjpZ2LiveScraper(string storageDirPath, ISjpIndex index)
            : base(storageDirPath, index)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//a[@class='job-inner']"))
            {
                yield return GetZ2LiveJd(new Uri(uri, jdLink.Attributes["href"].Value));
            }
        }

        private JobDescription GetZ2LiveJd(Uri uri)
        {
            HtmlNode jdNode = SjpUtils.GetHtmlDoc(uri).DocumentNode;

            string title = jdNode.SelectSingleNode("//h3[@class='career-listing-title']").InnerText;

            const string LocationPrefix = "Location:";
            string location = jdNode.SelectSingleNode("//strong[@class='location']").InnerText;
            int locationOffset = location.IndexOf(LocationPrefix) + LocationPrefix.Length;
            location = location.Substring(locationOffset);

            string description = jdNode.SelectSingleNode("//div[@class='ee-rich-text']").InnerHtml;

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