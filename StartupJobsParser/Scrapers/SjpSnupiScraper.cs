using System;

namespace StartupJobsParser
{
    public class SjpSnupiScraper : SjpResumatorScraperBase
    {
        private static readonly Uri _publicUri = new Uri("http://www.snupi.com/#jobs");
        private static readonly Uri _defaultScrapeUri = new Uri("http://app.theresumator.com/widgets/basic/create/snupi");

        public override string CompanyName { get { return "SNUPI"; } }
        public override Uri PublicUri { get { return _publicUri; } }
        public override Uri DefaultScrapeUri { get { return _defaultScrapeUri; } }

        public SjpSnupiScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}