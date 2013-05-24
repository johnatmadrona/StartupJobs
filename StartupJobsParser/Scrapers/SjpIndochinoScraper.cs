using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpIndochinoScraper : SjpScraper
    {
        private Uri _defaultUri = new Uri("http://www.indochino.com/about/careers");
        public override string CompanyName { get { return "Indochino"; } }
        public override Uri DefaultUri
        {
            get { return _defaultUri; }
        }

        public SjpIndochinoScraper(string storageDirPath, ISjpIndex index)
            : base(storageDirPath, index)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//ul[starts-with(@class,'positions')]/li/a"))
            {
                Uri jdUri = new Uri(uri, jdLink.Attributes["href"].Value);
                yield return GetIndochinoJd(jdUri);
            }
        }

        private JobDescription GetIndochinoJd(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);

            string title = doc.DocumentNode.SelectSingleNode("//article[@class='jobpost']/header/h1").InnerText;

            string location = doc.DocumentNode.SelectSingleNode("//article[@class='jobpost']/header/div").InnerText;
            if (location.Contains("|"))
            {
                location = location.Split('|')[1];
            }

            string description = doc.DocumentNode.SelectSingleNode("//section[@class='jobpost-content']").InnerText;

            return new JobDescription()
            {
                SourceUri = jdUri.AbsolutePath,
                Company = CompanyName,
                Title = WebUtility.HtmlDecode(title),
                Location = WebUtility.HtmlDecode(location),
                FullDescription = WebUtility.HtmlDecode(description)
            };
        }
    }
}