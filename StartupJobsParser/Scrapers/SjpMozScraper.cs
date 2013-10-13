using System;

namespace StartupJobsParser
{
    public class SjpMozScraper : SjpResumatorScraperBase
    {
        private Uri _publicUri = new Uri("http://moz.com/about/jobs");
        private Uri _defaultUri = new Uri("http://app.theresumator.com/widgets/basic/create/moz");

        public override string CompanyName { get { return "Moz"; } }

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

        public SjpMozScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}