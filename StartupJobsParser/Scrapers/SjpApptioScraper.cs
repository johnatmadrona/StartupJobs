using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpApptioScraper : SjpScraper
    {
        private Uri _defaultUri = new Uri("http://sj.tbe.taleo.net/CH18/ats/careers/searchResults.jsp?org=APPTIO&cws=1");
        public override string CompanyName { get { return "Apptio"; } }
        public override Uri DefaultUri
        {
            get { return _defaultUri; }
        }

        public SjpApptioScraper(string storageDirPath, ISjpIndex index)
            : base(storageDirPath, index)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//a[contains(@href, 'requisition.jsp')]"))
            {
                yield return GetApptioJd(new Uri(jdLink.Attributes["href"].Value));
            }
        }

        private JobDescription GetApptioJd(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);
            string title = doc.DocumentNode.SelectSingleNode("//section[@id='main']/table/tr[3]").InnerText;
            string location = doc.DocumentNode.SelectSingleNode("//section[@id='main']/table/tr[4]/td[2]").InnerText;
            string description = doc.DocumentNode.SelectSingleNode("//section[@id='main']/table/tr[7]").InnerText;

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = WebUtility.HtmlDecode(title),
                Location = WebUtility.HtmlDecode(location),
                FullDescription = WebUtility.HtmlDecode(description)
            };
        }
    }
}