using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

namespace StartupJobsParser
{
    public interface ISjpScraper
    {
        string DefaultUri { get; }
        void Scrape();
        void Scrape(string uri);
    }
}