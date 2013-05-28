using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpExtraHopScraper : SjpScraper
    {
        private Uri _defaultUri = new Uri("http://www.jobscore.com/jobs/extrahopnetworks");
        public override string CompanyName { get { return "ExtraHop"; } }
        public override Uri DefaultUri
        {
            get { return _defaultUri; }
        }

        public SjpExtraHopScraper(string storageDirPath, ISjpIndex index)
            : base(storageDirPath, index)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdListing in doc.DocumentNode.SelectNodes("//tr[@class='job-listing']"))
            {
                HtmlNode titleAndLink = jdListing.SelectSingleNode("td[@class='job-title']/a");
                HtmlNode location = jdListing.SelectSingleNode("td[@class='job-attribute']");
                yield return GetExtraHopJd(
                    WebUtility.HtmlDecode(titleAndLink.InnerText).Trim(),
                    WebUtility.HtmlDecode(location.InnerText).Trim(),
                    new Uri(titleAndLink.Attributes["href"].Value)
                    );
            }
        }

        private JobDescription GetExtraHopJd(string jobTitle, string jobLocation, Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);
            string description = doc.DocumentNode.SelectSingleNode("//div[@class='main_container']/div[starts-with(@class, 'left')]").InnerHtml;

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = jobTitle,
                Location = jobLocation,
                FullTextDescription = WebUtility.HtmlDecode(description).Trim(),
                FullHtmlDescription = description
            };
        }
    }
}