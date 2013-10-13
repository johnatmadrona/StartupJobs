using System;

namespace StartupJobsParser
{
    public class SjpSnupiScraper : SjpResumatorScraperBase
    {
        private Uri _publicUri = new Uri("http://www.snupi.com/#jobs");
        private Uri _defaultUri = new Uri("http://app.theresumator.com/widgets/basic/create/snupi");

        public override string CompanyName { get { return "SNUPI"; } }

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

        public SjpSnupiScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}