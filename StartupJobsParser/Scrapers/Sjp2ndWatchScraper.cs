using System;

namespace StartupJobsParser
{
    public class Sjp2ndWatchScraper : SjpResumatorScraperBase
    {
        private static readonly Uri _defaultScraperUri = new Uri("http://app.theresumator.com/widgets/basic/create/2ndwatch");
        private static readonly Uri _publicUri = new Uri("http://2ndwatch.com/contact-us/careers/");

        public override string CompanyName { get { return "2nd Watch"; } }
        public override Uri DefaultScrapeUri { get { return _defaultScraperUri; } }
        public override Uri PublicUri { get { return _publicUri; } }

        public Sjp2ndWatchScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}