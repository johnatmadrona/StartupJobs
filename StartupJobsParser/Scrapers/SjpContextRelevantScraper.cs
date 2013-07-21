using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpContextRelevantScraper : SjpScraper
    {
        private Uri _defaultUri = new Uri("http://contextrelevant.mytribehr.com/careers");
        public override string CompanyName { get { return "Context Relevant"; } }
        public override Uri DefaultUri
        {
            get { return _defaultUri; }
        }

        public SjpContextRelevantScraper(ISjpStorage storage, ISjpIndex index)
            : base(storage, index)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//div[@class='posting']/div[@class='description']/a"))
            {
                yield return GetContextRelevantJd(new Uri(uri, jdLink.Attributes["href"].Value));
            }
        }

        private JobDescription GetContextRelevantJd(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);
            HtmlNode jdNode = doc.DocumentNode.SelectSingleNode("//div[starts-with(@class,'jobPostings')]");

            HtmlNode titleNode = jdNode.SelectSingleNode("div[@class='title']/div[@class='box']/h2");
            HtmlNode descriptionNode = jdNode.SelectSingleNode("div[@class='posting description']");

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = "Seattle, WA",
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}