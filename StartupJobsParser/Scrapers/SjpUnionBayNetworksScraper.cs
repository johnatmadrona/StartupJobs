using System;

namespace StartupJobsParser
{
    public class SjpUnionBayNetworksScraper : SjpJobscoreScraperBase
    {
        private Uri _defaultUri = new Uri("http://www.jobscore.com/jobs/unionbaynetworks");
        private Uri _publicUri = new Uri("http://www.unionbaynetworks.com/");

        public override string CompanyName { get { return "Union Bay Networks"; } }

        public override Uri PublicUri
        {
            get
            {
                return _publicUri;
            }
        }

        public override Uri DefaultUri
        {
            get { return _defaultUri; }
        }

        public SjpUnionBayNetworksScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}