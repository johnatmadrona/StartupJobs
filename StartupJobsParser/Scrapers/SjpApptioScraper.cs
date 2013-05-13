using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpApptioScraper : SjpScraper
    {
        public override string CompanyName { get { return "Apptio"; } }
        public override string DefaultUri
        {
            get { return "http://sj.tbe.taleo.net/CH18/ats/careers/searchResults.jsp?org=APPTIO&cws=1"; }
        }

        public SjpApptioScraper(string storageDirPath, ISjpIndex index)
            : base(storageDirPath, index)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(string uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//a[contains(@href, 'requisition.jsp')]"))
            {
                yield return GetApptioJd(jdLink.Attributes["href"].Value);
            }
        }

        private JobDescription GetApptioJd(string jdLink)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdLink);
            string title = doc.DocumentNode.SelectSingleNode("//section[@id='main']/table/tr[3]").InnerText;
            string location = doc.DocumentNode.SelectSingleNode("//section[@id='main']/table/tr[4]/td[2]").InnerText;
            string description = doc.DocumentNode.SelectSingleNode("//section[@id='main']/table/tr[7]").InnerText;

            return new JobDescription()
            {
                SourceUri = jdLink,
                Company = CompanyName,
                Title = WebUtility.HtmlDecode(title),
                Location = WebUtility.HtmlDecode(location),
                FullDescription = WebUtility.HtmlDecode(description)
            };
        }
    }
}