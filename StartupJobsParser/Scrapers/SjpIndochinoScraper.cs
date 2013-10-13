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

        public SjpIndochinoScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//ul[starts-with(@class,'positions')]/li/a"))
            {
                Uri jdUri = new Uri(uri, jdLink.Attributes["href"].Value);
                JobDescription jd = GetIndochinoJd(jdUri);
                if (jd != null)
                {
                    yield return jd;
                }
            }
        }

        private JobDescription GetIndochinoJd(Uri jdUri)
        {
            HtmlDocument doc;
            try
            {
                doc = SjpUtils.GetHtmlDoc(jdUri);
            }
            catch (WebException ex)
            {
                Console.WriteLine("ERROR: Failed to retrieve URI '{0}' - {1}", jdUri, ex);
                return null;
            }

            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//article[@class='jobpost']/header/h1");

            string location = doc.DocumentNode.SelectSingleNode("//article[@class='jobpost']/header/div").InnerText;
            if (location.Contains("|"))
            {
                location = location.Split('|')[1];
            }

            HtmlNode descriptionNode = doc.DocumentNode.SelectSingleNode("//section[@class='jobpost-content']");

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = SjpUtils.GetCleanTextFromHtmlEncodedText(location),
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}