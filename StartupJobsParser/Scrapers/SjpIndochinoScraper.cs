using HtmlAgilityPack;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpIndochinoScraper : SjpScraper
    {
        public override string CompanyName { get { return "Indochino"; } }
        public override string DefaultUri
        {
            get { return "http://www.indochino.com/about/careers"; }
        }

        public SjpIndochinoScraper(string storageDirPath, ISjpIndex index)
            : base(storageDirPath, index)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(string uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//ul[starts-with(@class,'positions')]/li/a"))
            {
                yield return GetIndochinoJd(jdLink.Attributes["href"].Value);
            }
        }

        private JobDescription GetIndochinoJd(string jdLink)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdLink);

            string title = doc.DocumentNode.SelectSingleNode("//article[@class='jobpost']/header/h1").InnerText;

            string location = doc.DocumentNode.SelectSingleNode("//article[@class='jobpost']/header/div").InnerText;
            if (location.Contains("|"))
            {
                location = location.Split('|')[1];
            }

            string description = doc.DocumentNode.SelectSingleNode("//section[@class='jobpost-content']").InnerText;

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