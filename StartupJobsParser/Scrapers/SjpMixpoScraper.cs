using System;

namespace StartupJobsParser
{
    public class SjpMixpoScraper : SjpJobviteScraperBase
    {
        private static readonly Uri _publicUri = new Uri("http://dynamicvideoad.mixpo.com/about/careers/");

        public override string CompanyName { get { return "Mixpo"; } }
        protected override string JobviteCompanyId { get { return "qc3aVfw4"; } }

        public override Uri PublicUri { get { return _publicUri; } }

        public SjpMixpoScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}