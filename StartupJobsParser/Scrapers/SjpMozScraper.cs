using System;

namespace StartupJobsParser
{
    public class SjpMozScraper : SjpResumatorScraperBase
    {
        private static readonly Uri _publicUri = new Uri("http://moz.com/about/jobs");
        private static readonly Uri _defaultScrapeUri = new Uri("http://app.theresumator.com/widgets/basic/create/moz");

        public override string CompanyName { get { return "Moz"; } }
        public override Uri PublicUri { get { return _publicUri; } }
        public override Uri DefaultScrapeUri { get { return _defaultScrapeUri; } }

        public SjpMozScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}