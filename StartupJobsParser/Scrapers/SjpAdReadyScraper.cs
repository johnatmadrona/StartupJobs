using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpAdReadyScraper : SjpScraper
    {
        private Uri _defaultUri = new Uri("https://www.smartrecruiters.com/AdReadyInc");
        public override string CompanyName { get { return "AdReady"; } }
        public override Uri DefaultUri
        {
            get { return _defaultUri; }
        }

        public SjpAdReadyScraper(string storageDirPath, ISjpIndex index)
            : base(storageDirPath, index)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdInfoNode in doc.DocumentNode.SelectNodes("//table[@class='cs_table']/tr[@class='cs_container']"))
            {
                HtmlNode jdLink = jdInfoNode.SelectSingleNode("td[@class='cs_table']/a");
                yield return GetAdReadyJd(new Uri(uri, jdLink.Attributes["href"].Value));
            }
        }

        private JobDescription GetAdReadyJd(Uri uri)
        {
            HtmlNode jdNode = SjpUtils.GetHtmlDoc(uri).DocumentNode;

            string title = jdNode.SelectSingleNode("//h1[@class='jobAdTitle']").InnerText;
            string location = jdNode.SelectSingleNode("//span[@class='jobAdLocation']").InnerText;

            HtmlNode descriptionNode = jdNode.SelectSingleNode("//div[@class='jobAdLeft']");
            HtmlNode remove = descriptionNode.SelectSingleNode("input");
            remove.ParentNode.RemoveChild(remove, false);
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