using System;

namespace StartupJobsParser
{
    public class SjpWonderWorkshopScraper : SjpResumatorScraperBase
    {
        private static readonly Uri _publicUri = new Uri("https://www.makewonder.com/careers");
        private static readonly Uri _defaultScrapeUri = new Uri("http://app.theresumator.com/widgets/basic/create/Playi");

        public override string CompanyName { get { return "Wonder Workshop"; } }
        public override Uri PublicUri { get { return _publicUri; } }
        public override Uri DefaultScrapeUri { get { return _defaultScrapeUri; } }

        public SjpWonderWorkshopScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}