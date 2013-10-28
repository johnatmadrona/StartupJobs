using System;

namespace StartupJobsParser
{
    public class SjpWildTangentScraper : SjpTaleoScraperBase
    {
        private static readonly Uri _defaultScrapeUri = 
            new Uri("http://ch.tbe.taleo.net/CH02/ats/careers/searchResults.jsp?org=WILDTANGENT&cws=4");
        private static readonly Uri _publicUri = new Uri("http://www.wildtangent.com/Corporate/work-here/");

        public override string CompanyName { get { return "Wild Tangent"; } }
        public override Uri DefaultScrapeUri { get { return _defaultScrapeUri; } }
        public override Uri PublicUri { get { return _publicUri; } }

        protected override string JdContentTableXPath
        {
            get { return "//div[@id='taleoContent']/table"; }
        }

        public SjpWildTangentScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }
    }
}