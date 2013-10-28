using System;

namespace StartupJobsParser
{
    public class SjpRoverScraper : SjpResumatorScraperBase
    {
        private static readonly Uri _publicUri = new Uri("http://jobs.rover.com/");
        private static readonly Uri _defaultScrapeUri = new Uri("http://app.theresumator.com/widgets/basic/create/rover");

        public override string CompanyName { get { return "Rover.com"; } }
        public override Uri PublicUri { get { return _publicUri; } }
        public override Uri DefaultScrapeUri { get { return _defaultScrapeUri; } }

        public SjpRoverScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}