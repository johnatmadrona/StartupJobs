using StartupJobsParser;

namespace StartupJobsParser
{
    public class SjpScraperParams
    {
        public ISjpStorage Storage { get; set; }
        public ISjpIndex Index { get; set; }
        public ISjpLinkTracker LinkTracker { get; set; }
    }
}