using System;

namespace StartupJobsParser
{
    public class SjpLumoScraper : SjpJobscoreScraperBase
    {
        private Uri _defaultUri = new Uri("http://www.jobscore.com/jobs/lumobodytech");
        private Uri _publicUri = new Uri("http://www.lumoback.com/");

        public override string CompanyName { get { return "LUMO"; } }

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

        public SjpLumoScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}