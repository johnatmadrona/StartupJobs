using System;

namespace StartupJobsParser
{
    public class SjpExtraHopScraper : SjpJobscoreScraperBase
    {
        private static readonly Uri _defaultUri = new Uri("http://www.jobscore.com/jobs/extrahopnetworks");
        private static readonly Uri _publicUri = new Uri("http://www.extrahop.com/company/jobs/");

        public override string CompanyName { get { return "ExtraHop"; } }
        public override Uri PublicUri { get { return _publicUri; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }

        public SjpExtraHopScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}