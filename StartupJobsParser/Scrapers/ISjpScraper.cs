using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

namespace StartupJobsParser
{
    public interface ISjpScraper
    {
        Uri DefaultScrapeUri { get; }
        Uri PublicUri { get; }
        ScrapeResult Scrape();
        ScrapeResult Scrape(Uri uri);
    }
}