using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace StartupJobsParser
{
    public class SjpQumuloScraper : SjpScraper
    {
        private Uri _defaultUri = new Uri("http://qumulo.catsone.com/careers/");
        public override string CompanyName { get { return "Qumulo"; } }
        public override Uri DefaultUri
        {
            get { return _defaultUri; }
        }

        public SjpQumuloScraper(ISjpStorage storage, ISjpIndex index)
            : base(storage, index)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdNode in doc.DocumentNode.SelectNodes("//a[@class='jobTitle']"))
            {
                yield return GetQumuloJd(new Uri(jdNode.Attributes["href"].Value));
            }
        }

        private JobDescription GetQumuloJd(Uri uri)
        {
            HtmlNode jdNode = SjpUtils.GetHtmlDoc(uri).DocumentNode;
            HtmlNode titleNode = jdNode.SelectSingleNode("//h1[@id='jobTitle']");
            HtmlNode locationNode = jdNode.SelectSingleNode("//div[@id='jobDetailLocation']/strong");
            HtmlNode descriptionNode = jdNode.SelectSingleNode("//div[@class='detailsJobDescription']");

            return new JobDescription()
            {
                SourceUri = uri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = SjpUtils.GetCleanTextFromHtml(locationNode),
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}