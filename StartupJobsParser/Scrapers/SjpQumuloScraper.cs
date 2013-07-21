using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpQumuloScraper : SjpScraper
    {
        private Uri _defaultUri = new Uri("http://www.qumulo.com/#/qumulo_jobs_eng");
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
            foreach (HtmlNode jdNode in doc.DocumentNode.SelectNodes("//div[@data-position]"))
            {
                yield return GetQumuloJd(jdNode, uri);
            }
        }

        private JobDescription GetQumuloJd(HtmlNode jdNode, Uri uri)
        {
            string title = jdNode.Attributes["data-position"].Value;
            HtmlNode descriptionNode = jdNode.SelectSingleNode("div[@class='jdbg']/div[@class='body']/span");

            return new JobDescription()
            {
                SourceUri = uri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtmlEncodedText(title),
                Location = "Seattle, WA",
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}