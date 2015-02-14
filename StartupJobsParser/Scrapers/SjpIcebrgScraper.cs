using System;

namespace StartupJobsParser
{
    public class SjpIcebrgScraper : SjpAngelListScraperBase
    {
        private static readonly Uri _defaultUri = new Uri("https://angel.co/icebrg-io/jobs");

        public override string CompanyName { get { return "Icebrg"; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }
        public override Uri PublicUri { get { return _defaultUri; } }

        public SjpIcebrgScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}