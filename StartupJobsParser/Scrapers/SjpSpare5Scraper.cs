using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace StartupJobsParser
{
    public class SjpSpare5Scraper : SjpScraper
    {
        private static readonly Uri _defaultScrapeUri = new Uri("http://www.spare5.com/jobs/");
        private const string _defaultLocation = "Seattle, WA";

        public override string CompanyName { get { return "Spare5"; } }
        public override Uri DefaultScrapeUri { get { return _defaultScrapeUri; } }
        public override Uri PublicUri { get { return _defaultScrapeUri; } }

        public SjpSpare5Scraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri, true, null);

            HtmlNode jobsSectionNode = doc.DocumentNode.SelectSingleNode("//div[@id='jobsbody-page']");
            HtmlNodeCollection jobSectionNodes = jobsSectionNode.SelectNodes(".//div[@data-block-type='2']");

            if (jobSectionNodes.Count > 0)
            {
                // Remove nodes that aren't job descriptions (i.e. ones that don't contain an h2, which holds job title)
                for (int i = 0; i < jobSectionNodes.Count; i++)
                {
                    if (jobSectionNodes[i].SelectSingleNode(".//h2") == null)
                    {
                        jobSectionNodes.Remove(i);
                        i--;
                    }
                }
            }
            if (jobSectionNodes.Count > 0)
            {
                // The first job description is not actually a job, but a culture description
                jobSectionNodes.Remove(0);
            }

            foreach (HtmlNode jobSectionNode in jobSectionNodes)
            {
                HtmlNode jdNode = jobSectionNode.SelectSingleNode(".//div[@class='sqs-block-content']");
                yield return GetSpare5Jd(jdNode);
            }
        }

        private JobDescription GetSpare5Jd(HtmlNode jdNode)
        {
            HtmlNode titleNode = jdNode.SelectSingleNode(".//h2");
            string title = SjpUtils.GetCleanTextFromHtml(titleNode);
            titleNode.Remove();

            return new JobDescription()
            {
                SourceUri = _defaultScrapeUri.AbsoluteUri,
                Company = CompanyName,
                Title = title,
                Location = _defaultLocation,
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(jdNode),
                FullHtmlDescription = jdNode.InnerHtml
            };
        }
    }
}