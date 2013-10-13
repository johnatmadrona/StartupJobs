using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpAnimotoScraper : SjpJobviteScraperBase
    {
        public override string CompanyName { get { return "Animoto"; } }
        protected override string JobviteCompanyId { get { return "qBr9VfwQ"; } }

        public SjpAnimotoScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdUriNode in doc.DocumentNode.SelectNodes("//table[@class='joblist']/tr/td/a[starts-with(@onclick,'jvGoToPage')]"))
            {
                yield return GetAnimotoJd(ExtractJdUriFromGoToPageLink(jdUriNode));
            }
        }

        private JobDescription GetAnimotoJd(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);

            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//div[@class='jvjobheader']/h2");

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

            HtmlNode descriptionNode = doc.DocumentNode.SelectSingleNode("//div[@class='jvdescriptionbody']");

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = WebUtility.HtmlDecode(location).Trim(),
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}