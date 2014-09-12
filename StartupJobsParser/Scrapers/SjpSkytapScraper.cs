using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpSkytapScraper : SjpJobviteScraperBase
    {
        //private static readonly Uri _defaultUri = new Uri("http://www.skytap.com/company/careers/?/about-us/careers/index.php");

        private static readonly Uri _publicUri = new Uri("http://www.skytap.com/company/careers");
        
        public override string CompanyName { get { return "Skytap"; } }
        protected override string JobviteCompanyId { get { return "q1A9Vfwp"; } }
        public override Uri PublicUri { get { return _publicUri; } }

        public SjpSkytapScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            /*HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//a[starts-with(@href, 'http://www.skytap.com/company/careers/')]"))
            {
                yield return GetSkytapJd(new Uri(jdLink.Attributes["href"].Value));
            }*/
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdUriNode in doc.DocumentNode.SelectNodes("//ul[@class='joblist']/li/a[starts-with(@onclick,'jvGoToPage')]"))
            {
                yield return GetSkytapJd(ExtractJdUriFromGoToPageLink(jdUriNode));
            }
        }

        private JobDescription GetSkytapJd(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);

            HtmlNode headerNode = doc.DocumentNode.SelectSingleNode("//div[@class='jvjobheader']");
            HtmlNode titleNode = headerNode.SelectSingleNode("h2");
            HtmlNode locationNode = headerNode.SelectSingleNode("h3");
            HtmlNode descNode = doc.DocumentNode.SelectSingleNode("//div[@class='jvdescriptionbody']");

            string location = SjpUtils.GetCleanTextFromHtml(locationNode);
            if (location.Contains("|"))
            {
                location = location.Split('|')[1].Trim();
            }

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = location,
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descNode),
                FullHtmlDescription = descNode.InnerHtml
            };
        }
    }
}