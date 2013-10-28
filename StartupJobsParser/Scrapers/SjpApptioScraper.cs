using System;

namespace StartupJobsParser
{
    public class SjpApptioScraper : SjpTaleoScraperBase
    {
        private static readonly Uri _defaultScrapeUri = 
            new Uri("http://sj.tbe.taleo.net/CH18/ats/careers/searchResults.jsp?org=APPTIO&cws=1");
        private static readonly Uri _publicUri = new Uri("http://www.apptio.com/company/careers");

        public override string CompanyName { get { return "Apptio"; } }
        public override Uri DefaultScrapeUri { get { return _defaultScrapeUri; } }
        public override Uri PublicUri { get { return _publicUri; } }

        protected override string JdContentTableXPath
        {
            get { return "//section[@id='main']/table"; }
        }

        public SjpApptioScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}