using HtmlAgilityPack;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpExtraHopScraper : SjpScraper
    {
        public override string CompanyName { get { return "ExtraHop"; } }
        public override string DefaultUri
        {
            get { return "http://www.extrahop.com/company/jobs/"; }
        }

        public SjpExtraHopScraper(string storageDirPath, ISjpIndex index)
            : base(storageDirPath, index)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(string uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//div[@class='entry']/ul/li/a[starts-with(@href, 'http://www.extrahop.com/company/jobs/')]"))
            {
                yield return GetExtraHopJd(jdLink.Attributes["href"].Value);
            }
        }

        private JobDescription GetExtraHopJd(string jdLink)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdLink);
            string title = doc.DocumentNode.SelectSingleNode("//div[@id='Content']/h1").InnerText;
            string description = doc.DocumentNode.SelectSingleNode("//div[@class='entry']").InnerText;

            return new JobDescription()
            {
                SourceUri = jdLink,
                Company = CompanyName,
                Title = WebUtility.HtmlDecode(title),
                Location = "Seattle, WA",
                FullDescription = WebUtility.HtmlDecode(description)
            };
        }
    }
}