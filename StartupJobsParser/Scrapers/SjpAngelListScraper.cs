using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace StartupJobsParser
{
    public class SjpAngelListScraper : SjpScraper
    {
        private Uri _defaultUri;
        private string _companyName;

        public override string CompanyName { get { return _companyName; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }
        public override Uri PublicUri { get { return _defaultUri; } }

        public SjpAngelListScraper(
            SjpScraperParams scraperParams,
            string companyName,
            string uri
            )
            : this(scraperParams, companyName, new Uri(uri))
        {
        }

        public SjpAngelListScraper(
            SjpScraperParams scraperParams,
            string companyName,
            Uri uri
            )
            : base(scraperParams)
        {
            // TODO: Refactor. Assignment of variable name here is bad since base class may try to access.
            _defaultUri = uri;
            _companyName = companyName;
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode linkNode in doc.DocumentNode.SelectNodes("//a[contains(@class,'job-link')]"))
            {
                yield return GetAngelListJd(new Uri(uri, linkNode.Attributes["href"].Value));
            }
        }

        private JobDescription GetAngelListJd(Uri jdUri)
        {
            HtmlNode doc = SjpUtils.GetHtmlDoc(jdUri).DocumentNode;

            HtmlNode titleNode = doc.SelectSingleNode("//h1[contains(@class,'join-title')]");

            HtmlNodeCollection parts = doc.SelectNodes("//div[@class='s-vgPad2']");

            // Expect to always get 2 items, 1st item is job description, 2nd item is job metadata
            HtmlNode descriptionNode = parts[0];

            string location = null;
            HtmlNodeCollection jobDescriptors = parts[1].SelectNodes("./div");
            for (int i = 0; i < jobDescriptors.Count && location == null; i++)
            {

                if (SjpUtils.GetCleanTextFromHtml(jobDescriptors[i]) == "Location")
                {
                    location = SjpUtils.GetCleanTextFromHtml(jobDescriptors[i + 1]);

                    const string remoteText = ", Remote OK";
                    int remoteIndex = location.IndexOf(remoteText);
                    if (remoteIndex > 0)
                    {
                        location = location.Remove(remoteIndex, remoteText.Length);
                    }
                }
            }
            if (location == null)
            {
                throw new Exception("Could not find location");
            }

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = location,
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}