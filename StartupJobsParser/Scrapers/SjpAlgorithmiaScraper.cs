using System;

namespace StartupJobsParser
{
    public class SjpAlgorithmiaScraper : SjpAngelListScraperBase
    {
        private static readonly Uri _defaultUri = new Uri("https://angel.co/algorithmia/jobs");

        public override string CompanyName { get { return "Algorithmia"; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }
        public override Uri PublicUri { get { return _defaultUri; } }

        public SjpAlgorithmiaScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}