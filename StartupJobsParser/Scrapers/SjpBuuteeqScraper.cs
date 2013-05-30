using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpBuuteeqScraper : SjpScraper
    {
        private Uri _defaultUri = new Uri("http://jobs.buuteeq.me/promotions.htm");
        public override string CompanyName { get { return "buuteeq"; } }
        public override Uri DefaultUri
        {
            get { return _defaultUri; }
        }

        public SjpBuuteeqScraper(string storageDirPath, ISjpIndex index)
            : base(storageDirPath, index)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode categoryLink in doc.DocumentNode.SelectNodes("//div[@id='sideBar']/div/ul/li/a"))
            {
                foreach (JobDescription jd in GetJdsFromCategory(new Uri(uri, categoryLink.Attributes["href"].Value)))
                {
                    yield return jd;
                }
            }
        }

        protected IEnumerable<JobDescription> GetJdsFromCategory(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//a[contains(@href, '/current-openings/')]"))
            {
                yield return GetBuuteeqJd(new Uri(uri, jdLink.Attributes["href"].Value));
            }
        }

        private JobDescription GetBuuteeqJd(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);
            string title = doc.DocumentNode.SelectSingleNode("//div[@class='artMainCon']/h1").InnerText;
            string description = doc.DocumentNode.SelectSingleNode("//div[@class='artMainCon']").InnerHtml;

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = WebUtility.HtmlDecode(title).Trim(),
                Location = "Seattle, WA",
                FullTextDescription = WebUtility.HtmlDecode(description).Trim(),
                FullHtmlDescription = description
            };
        }
    }
}