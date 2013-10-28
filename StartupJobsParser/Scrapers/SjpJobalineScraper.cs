using System;

namespace StartupJobsParser
{
    public class SjpJobalineScraper : SjpResumatorScraperBase
    {
        private static readonly Uri _publicScrapeUri = new Uri("http://jobalinecareers.theresumator.com/");
        private static readonly Uri _defaultUri = new Uri("http://app.theresumator.com/widgets/basic/create/jobalinecareers");

        public override string CompanyName { get { return "Jobaline"; } }
        public override Uri PublicUri { get { return _publicScrapeUri; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }

        public SjpJobalineScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}