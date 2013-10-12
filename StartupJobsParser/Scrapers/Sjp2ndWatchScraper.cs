using System;

namespace StartupJobsParser
{
    public class Sjp2ndWatchScraper : SjpResumatorScraperBase
    {
        private Uri _publicUri = new Uri("http://2ndwatch.com/contact-us/careers/");
        private Uri _defaultUri = new Uri("http://app.theresumator.com/widgets/basic/create/2ndwatch");

        public override string CompanyName { get { return "2nd Watch"; } }

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

        public Sjp2ndWatchScraper(ISjpStorage storage, ISjpIndex index)
            : base(storage, index)
        {
        }
    }
}