using System;

namespace StartupJobsParser
{
    public class SjpJulepScraper : SjpResumatorScraperBase
    {
        private static readonly Uri _publicUri = new Uri("http://www.julep.com/careers.html");
        private static readonly Uri _defaultScrapeUri = new Uri("http://app.theresumator.com/widgets/basic/create/julepbeauty");

        public override string CompanyName { get { return "Julep"; } }
        public override Uri PublicUri { get { return _publicUri; } }
        public override Uri DefaultScrapeUri { get { return _defaultScrapeUri; } }

        public SjpJulepScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}