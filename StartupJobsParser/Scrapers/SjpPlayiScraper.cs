using System;

namespace StartupJobsParser
{
    public class SjpPlayiScraper : SjpResumatorScraperBase
    {
        private static readonly Uri _publicUri = new Uri("https://www.play-i.com/careers");
        private static readonly Uri _defaultScrapeUri = new Uri("http://app.theresumator.com/widgets/basic/create/Playi");

        public override string CompanyName { get { return "play-i"; } }
        public override Uri PublicUri { get { return _publicUri; } }
        public override Uri DefaultScrapeUri { get { return _defaultScrapeUri; } }

        public SjpPlayiScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}