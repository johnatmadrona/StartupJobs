using System;

namespace StartupJobsParser
{
    public class SjpCheezburgerScraper : SjpResumatorScraperBase
    {
        private static readonly Uri _publicUri = new Uri("http://jobs.cheezburger.com/");
        private static readonly Uri _defaultScrapeUri = new Uri("http://app.theresumator.com/widgets/basic/create/cheezburger");

        public override string CompanyName { get { return "Cheezburger"; } }
        public override Uri PublicUri { get { return _publicUri; } }
        public override Uri DefaultScrapeUri { get { return _defaultScrapeUri; } }

        public SjpCheezburgerScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}