using System;

namespace StartupJobsParser
{
    public class SjpRoverScraper : SjpResumatorScraperBase
    {
        private Uri _publicUri = new Uri("http://jobs.rover.com/");
        private Uri _defaultUri = new Uri("http://app.theresumator.com/widgets/basic/create/rover");

        public override string CompanyName { get { return "Rover.com"; } }

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

        public SjpRoverScraper(ISjpStorage storage, ISjpIndex index)
            : base(storage, index)
        {
        }
    }
}