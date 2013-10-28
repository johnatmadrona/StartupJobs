using System;

namespace StartupJobsParser
{
    public class SjpUnionBayNetworksScraper : SjpJobscoreScraperBase
    {
        private static readonly Uri _defaultScrapeUri = new Uri("http://www.jobscore.com/jobs/unionbaynetworks");
        private static readonly Uri _publicUri = new Uri("http://www.unionbaynetworks.com/");

        public override string CompanyName { get { return "Union Bay Networks"; } }
        public override Uri PublicUri { get { return _publicUri; } }
        public override Uri DefaultScrapeUri { get { return _defaultScrapeUri; } }

        public SjpUnionBayNetworksScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}