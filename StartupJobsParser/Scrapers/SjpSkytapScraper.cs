using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpSkytapScraper : SjpJobviteScraperBase
    {
        private static readonly Uri _publicUri = new Uri("http://www.skytap.com/company/careers");
        
        public override string CompanyName { get { return "Skytap"; } }
        protected override string JobviteCompanyId { get { return "q1A9Vfwp"; } }
        public override Uri PublicUri { get { return _publicUri; } }

        public SjpSkytapScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}