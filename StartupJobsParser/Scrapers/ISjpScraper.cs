using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

namespace StartupJobsParser
{
    public interface ISjpScraper
    {
        Uri DefaultUri { get; }
        void Scrape();
        void Scrape(Uri uri);
    }
}