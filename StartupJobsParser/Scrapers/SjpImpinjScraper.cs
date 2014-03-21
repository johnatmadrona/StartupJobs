using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpImpinjScraper : SjpJobviteScraperBase
    {
        private static readonly Uri _publicUri = new Uri("http://www.impinj.com/careers/");

        public override string CompanyName { get { return "Impinj"; } }
        protected override string JobviteCompanyId { get { return "qPD9Vfwg"; } }

        public override Uri PublicUri { get { return _publicUri; } }

        public SjpImpinjScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdUriNode in doc.DocumentNode.SelectNodes("//div[@class='jobList']/ul/li/a[contains(@href,'?jvi=')]"))
            {
                yield return GetImpinjJd(ExtractJdUriFromGoToPageLink(jdUriNode));
            }
        }

        private JobDescription GetImpinjJd(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);

            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//div[@class='title_jobdesc']/h2");

            HtmlNode locationNode = titleNode.NextSibling;
            string location = null;
            while (location == null)
            {
                if (locationNode.InnerText.Contains("|"))
                {
                    location = locationNode.InnerText.Split('|')[1];
                }
                else
                {
                    locationNode = locationNode.NextSibling;
                }
            }

            HtmlNode descriptionNode = doc.DocumentNode.SelectSingleNode("//div[@class='jobDesc']");

            return new JobDescription()
            {
                SourceUri = TryCreateTrackedLink(PublicTaggedUri),
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = WebUtility.HtmlDecode(location).Trim(),
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}