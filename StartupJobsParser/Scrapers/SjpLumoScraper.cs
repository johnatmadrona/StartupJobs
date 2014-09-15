using System;

namespace StartupJobsParser
{
    public class SjpLumoScraper : SjpJobscoreScraperBase
    {
        private static readonly Uri _defaultScrapeUri = new Uri("http://www.jobscore.com/jobs/lumobodytech");
        private static readonly Uri _publicUri = new Uri("http://www.lumoback.com/");

        public override string CompanyName { get { return "LUMO BodyTech"; } }
        public override Uri PublicUri { get { return _publicUri; } }
        public override Uri DefaultScrapeUri { get { return _defaultScrapeUri; } }

        public SjpLumoScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}