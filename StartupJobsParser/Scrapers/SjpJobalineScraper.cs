using System;

namespace StartupJobsParser
{
    public class SjpJobalineScraper : SjpResumatorScraperBase
    {
        private Uri _publicUri = new Uri("http://jobalinecareers.theresumator.com/");
        private Uri _defaultUri = new Uri("http://app.theresumator.com/widgets/basic/create/jobalinecareers");

        public override string CompanyName { get { return "Jobaline"; } }

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

        public SjpJobalineScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}