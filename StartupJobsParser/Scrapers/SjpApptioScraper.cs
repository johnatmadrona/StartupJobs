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
            HtmlNode jdNode = doc.DocumentNode.SelectSingleNode("//section[@id='main']/table");

            HtmlNode titleNode = jdNode.SelectSingleNode("tr/td/h1");
            HtmlNode locationNode = jdNode.SelectSingleNode("tr/td/b");

            // TODO: We're losing some data like the wrapping table...
            HtmlNode descriptionNode = jdNode.SelectSingleNode("tr");
            while (!SjpUtils.GetCleanTextFromHtml(descriptionNode).StartsWith("Description", StringComparison.OrdinalIgnoreCase))
            {
                descriptionNode = descriptionNode.NextSibling;
            }

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = SjpUtils.GetCleanTextFromHtml(locationNode),
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}