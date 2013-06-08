using System;

namespace StartupJobsParser
{
    public class SjpWildTangentScraper : SjpTaleoScraperBase
    {
        private Uri _defaultUri = new Uri("http://ch.tbe.taleo.net/CH02/ats/careers/searchResults.jsp?org=WILDTANGENT&cws=4");

        public override string CompanyName { get { return "Wild Tangent"; } }

        public override Uri DefaultUri
        {
            get { return _defaultUri; }
        }

        protected override string JdContentTableXPath
        {
            get { return "//div[@id='taleoContent']/table"; }
        }

        public SjpWildTangentScraper(string storageDirPath, ISjpIndex index)
            : base(storageDirPath, index)
        {
        }
    }
}