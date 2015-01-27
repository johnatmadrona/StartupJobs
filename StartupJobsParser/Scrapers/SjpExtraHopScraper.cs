using System;

namespace StartupJobsParser
{
    public class SjpExtraHopScraper : SjpJobviteScraperBase
    {        private static readonly Uri _publicUri = new Uri("http://www.extrahop.com/company/jobs/");        public override string CompanyName { get { return "ExtraHop"; } }        protected override string JobviteCompanyId { get { return "qAYaVfwn"; } }        public override Uri PublicUri { get { return _publicUri; } }

        public SjpExtraHopScraper(SjpScraperParams scraperParams)            : base(scraperParams)        {        }
    }
}