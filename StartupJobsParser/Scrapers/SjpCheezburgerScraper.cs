using System;

namespace StartupJobsParser
{
    public class SjpCheezburgerScraper : SjpResumatorScraperBase
    {
        private Uri _publicUri = new Uri("http://jobs.cheezburger.com/");
        private Uri _defaultUri = new Uri("http://app.theresumator.com/widgets/basic/create/cheezburger");

        public override string CompanyName { get { return "Cheezburger"; } }

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

        public SjpCheezburgerScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}