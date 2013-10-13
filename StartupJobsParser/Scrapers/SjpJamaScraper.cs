using System;

namespace StartupJobsParser
{
    public class SjpJamaScraper : SjpJobscoreScraperBase
    {
        private Uri _defaultUri = new Uri("http://www.jobscore.com/jobs/jamasoftware/");
        private Uri _publicUri = new Uri("http://www.jamasoftware.com/company/#careers");

        public override string CompanyName { get { return "Jama"; } }

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

        public SjpJamaScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}