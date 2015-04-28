using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StartupJobsParser
{
    public class SjpGreenhouseScraper : SjpScraper
    {
        private string _companyName;
        private string _greenhouseId;
        private Uri _publicUri;
        private Uri _scrapeUri;

        public override string CompanyName { get { return _companyName; } }
        public override Uri PublicUri { get { return _publicUri; } }
        public override Uri DefaultScrapeUri { get { return _scrapeUri; } }

        const string BoardUrlFormat = "https://boards.greenhouse.io/embed/job_board?for={0}";
        const string JobUrlFormat = "https://boards.greenhouse.io/embed/job_app?for={0}&token={1}";

        public SjpGreenhouseScraper(
            SjpScraperParams scraperParams,
            string companyName,
            string publicUri,
            string greenhouseId
            )
            : base(scraperParams)
        {
            // TODO: Refactor. Assignment of variable name here is bad since base class may try to access.
            _companyName = companyName;
            _greenhouseId = greenhouseId;
            _publicUri = new Uri(publicUri);
            _scrapeUri = new Uri(string.Format(BoardUrlFormat, _greenhouseId));
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdNode in doc.DocumentNode.SelectNodes("//div[@class='opening']/a"))
            {
                string jobUrl = jdNode.Attributes["href"].Value;
                Regex rx = new Regex(@"gh_jid=(?<JobId>\d+)");
                Match m = rx.Match(jobUrl);
                if (!m.Success)
                {
                    throw new Exception("Unexpected format: " + jobUrl);
                }
                string ghJobUrl = string.Format(JobUrlFormat, _greenhouseId, m.Groups["JobId"].Value);
                yield return GetGreenhouseJd(new Uri(ghJobUrl));
            }
        }

        protected JobDescription GetGreenhouseJd(Uri jobUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jobUri);
            HtmlNode jobNode = doc.DocumentNode.SelectSingleNode("//div[@id='app_body']");

            HtmlNode titleNode = jobNode.SelectSingleNode("//*[@class='app-title']");
            HtmlNode locationNode = jobNode.SelectSingleNode("//*[@class='location']");
            HtmlNode descriptionNode = jobNode.SelectSingleNode("//*[@id='content']");

            return new JobDescription()
            {
                SourceUri = jobUri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = SjpUtils.GetCleanTextFromHtml(locationNode),
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}