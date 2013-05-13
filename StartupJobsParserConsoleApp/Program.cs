using System;
using System.Collections.Generic;
using StartupJobsParser;
using System.IO;

namespace StartupJobsParserConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //string indexDirPath = Directory.GetCurrentDirectory() + "\\lucene\\";
            ISjpIndex index = new SjpLocalDiskIndex(Path.GetFullPath(".\\lucene\\"));

            List<ISjpScraper> scrapers = new List<ISjpScraper>();
            //scrapers.Add(new SjpApptioScraper(".\\apptio\\", index));
            //scrapers.Add(new SjpRedfinScraper(".\\redfin\\", index));
            //scrapers.Add(new SjpExtraHopScraper(".\\extrahop\\", index));
            //scrapers.Add(new SjpIndochinoScraper(".\\indochino\\", index));
            //scrapers.Add(new SjpSmartsheetScraper(".\\smartsheet\\", index));

            foreach (ISjpScraper scraper in scrapers)
            {
                scraper.Scrape();
            }

            foreach (JobDescription jd in index.FindJds("java"))
            {
                SjpLogger.Log("Found: " + jd.Company + " - " + jd.Title);
            }

            Console.WriteLine("Done. Press <enter>.");
            Console.ReadLine();
        }
    }
}
