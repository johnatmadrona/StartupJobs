using System;

namespace StartupJobsParser
{
    public class SjpJamaScraper : SjpJobscoreScraperBase
    {
        private static readonly Uri _defaultScrapeUri = new Uri("http://www.jobscore.com/jobs/jamasoftware/");
        private static readonly Uri _publicUri = new Uri("http://www.jamasoftware.com/company/#careers");

        public override string CompanyName { get { return "Jama"; } }
        public override Uri PublicUri { get { return _publicUri; } }
        public override Uri DefaultScrapeUri { get { return _defaultScrapeUri; } }

        public SjpJamaScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}