using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpHaikuDeckScraper : SjpScraper
    {
        private Uri _defaultUri = new Uri("http://www.haikudeck.com/jobs");
        public override string CompanyName { get { return "Haiku Deck"; } }
        public override Uri DefaultUri
        {
            get { return _defaultUri; }
        }

        public SjpHaikuDeckScraper(string storageDirPath, ISjpIndex index)
            : base(storageDirPath, index)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdNode in doc.DocumentNode.SelectNodes("//div[@class='accordion']/div[@class='accordion-group']"))
            {
                yield return GetHaikuDeckJd(jdNode, uri);
            }
        }

        private JobDescription GetHaikuDeckJd(HtmlNode jdNode, Uri uri)
        {
            string title = jdNode.SelectSingleNode("div[@class='accordion-heading']").InnerText;
            string description = jdNode.SelectSingleNode("div/div[@class='accordion-inner']").InnerHtml;

            return new JobDescription()
            {
                SourceUri = uri.AbsoluteUri,
                Company = CompanyName,
                Title = WebUtility.HtmlDecode(title).Trim(),
                Location = "Fremont, WA",
                FullTextDescription = WebUtility.HtmlDecode(description).Trim(),
                FullHtmlDescription = description
            };
        }
    }
}