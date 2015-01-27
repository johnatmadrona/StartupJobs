using System;

namespace StartupJobsParser
{
    public class SjpISpotTvScraper : SjpJobviteScraperBase
    {
        private static readonly Uri _publicUri = new Uri("http://www.ispot.tv/careers");

        public override string CompanyName { get { return "iSpot.tv"; } }
        protected override string JobviteCompanyId { get { return "qrI9VfwX"; } }

        public override Uri PublicUri { get { return _publicUri; } }

        public SjpISpotTvScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}