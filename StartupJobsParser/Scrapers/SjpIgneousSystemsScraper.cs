using System;

namespace StartupJobsParser
{
    public class SjpIgneousSystemsScraper : SjpJobviteScraperBase
    {
        private static readonly Uri _publicUri = new Uri("http://hire.jobvite.com/CompanyJobs/Careers.aspx?k=JobListing&c=qpO9Vfw1&v=1");

        public override string CompanyName { get { return "Igneous Systems"; } }
        protected override string JobviteCompanyId { get { return "qpO9Vfw1"; } }

        public override Uri PublicUri { get { return _publicUri; } }

        public SjpIgneousSystemsScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}