using System;

namespace StartupJobsParser
{
    public class SjpApptioScraper : SjpTaleoScraperBase
    {
        private Uri _defaultUri = new Uri("http://sj.tbe.taleo.net/CH18/ats/careers/searchResults.jsp?org=APPTIO&cws=1");

        public override string CompanyName { get { return "Apptio"; } }

        public override Uri DefaultUri
        {
            get { return _defaultUri; }
        }

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