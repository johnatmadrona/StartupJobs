using StartupJobsParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace StartupJobsParserConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //string indexDirPath = Directory.GetCurrentDirectory() + "\\index\\";
            ISjpIndex index = null;// new SjpLocalDiskIndex(Path.GetFullPath(".\\index\\"));

            List<ISjpScraper> scrapers = new List<ISjpScraper>();
            scrapers.Add(new Sjp2ndWatchScraper(new SjpStorageDisk(".\\data\\2ndwatch\\"), index));
            scrapers.Add(new SjpAdReadyScraper(new SjpStorageDisk(".\\data\\adready\\"), index));
            scrapers.Add(new SjpApptioScraper(new SjpStorageDisk(".\\data\\apptio\\"), index));
            scrapers.Add(new SjpAnimotoScraper(new SjpStorageDisk(".\\data\\animoto\\"), index));
            scrapers.Add(new SjpBuuteeqScraper(new SjpStorageDisk(".\\data\\buuteeq\\"), index));
            scrapers.Add(new SjpCheezburgerScraper(new SjpStorageDisk(".\\data\\cheezburger\\"), index));
            scrapers.Add(new SjpContextRelevantScraper(new SjpStorageDisk(".\\data\\contextrelevant\\"), index));
            scrapers.Add(new SjpExtraHopScraper(new SjpStorageDisk(".\\data\\extrahop\\"), index));
            scrapers.Add(new SjpHaikuDeckScraper(new SjpStorageDisk(".\\data\\haikudeck\\"), index));
            scrapers.Add(new SjpImpinjScraper(new SjpStorageDisk(".\\data\\impinj\\"), index));
            scrapers.Add(new SjpIndochinoScraper(new SjpStorageDisk(".\\data\\indochino\\"), index));
            scrapers.Add(new SjpIntrepidLearningScraper(new SjpStorageDisk(".\\data\\intrepidlearning\\"), index));
            scrapers.Add(new SjpMozScraper(new SjpStorageDisk(".\\data\\moz\\"), index));
            scrapers.Add(new SjpMixpoScraper(new SjpStorageDisk(".\\data\\mixpo\\"), index));
            scrapers.Add(new SjpPayscaleScraper(new SjpStorageDisk(".\\data\\payscale\\"), index));
            scrapers.Add(new SjpPlacedScraper(new SjpStorageDisk(".\\data\\placed\\"), index));
            scrapers.Add(new SjpQumuloScraper(new SjpStorageDisk(".\\data\\qumulo\\"), index));
            scrapers.Add(new SjpRedfinScraper(new SjpStorageDisk(".\\data\\redfin\\"), index));
            scrapers.Add(new SjpRoverScraper(new SjpStorageDisk(".\\data\\rover\\"), index));
            scrapers.Add(new SjpSkytapScraper(new SjpStorageDisk(".\\data\\skytap\\"), index));
            scrapers.Add(new SjpSmartsheetScraper(new SjpStorageDisk(".\\data\\smartsheet\\"), index));
            scrapers.Add(new SjpTier3Scraper(new SjpStorageDisk(".\\data\\tier3\\"), index));
            scrapers.Add(new SjpWildTangentScraper(new SjpStorageDisk(".\\data\\wildtangent\\"), index));
            scrapers.Add(new SjpZ2LiveScraper(new SjpStorageDisk(".\\data\\z2live\\"), index));

            Parallel.ForEach(scrapers, scraper =>
            {
                scraper.Scrape();
            });

            //foreach (JobDescription jd in index.FindJds("java"))
            //{
            //    SjpLogger.Log("Found: " + jd.Company + " - " + jd.Title);
            //}

            Console.WriteLine("Done. Press <enter>.");
            Console.ReadLine();
        }
    }
}
