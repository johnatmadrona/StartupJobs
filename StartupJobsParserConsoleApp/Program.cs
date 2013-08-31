using Amazon;
using StartupJobsParser;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace StartupJobsParserConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["AWSAccessKey"]) ||
                string.IsNullOrEmpty(ConfigurationManager.AppSettings["AWSSecretKey"]))
            {
                Console.WriteLine("ERROR: Must add values for AWSAccessKey and AWSSecretKey in application config");
                return;
            }

            //SjpStorageDisk storage = new SjpStorageDisk(".\\data\\");
            SjpStorageS3 storage = new SjpStorageS3(RegionEndpoint.USWest2, "madrona-sjp");
            List<ISjpScraper> scrapers = GetScrapers(storage, GetIndex());

            List<KeyValuePair<Type, Exception>> errors = new List<KeyValuePair<Type,Exception>>();
            Parallel.ForEach(scrapers, scraper =>
            {
                try
                {
                    scraper.Scrape();
                }
                catch (Exception ex)
                {
                    errors.Add(new KeyValuePair<Type,Exception>(scraper.GetType(), ex));
                }
            });

            if (errors.Count > 0)
            {
                Console.WriteLine("\nERRORS:");
                Console.WriteLine("=======\n");
                foreach (var error in errors)
                {
                    Console.WriteLine("{0}:\n{1}\n", error.Key, error.Value);
                }
            }

            Console.WriteLine("Done. Press <enter>.");
            Console.ReadLine();
        }

        public static ISjpIndex GetIndex()
        {
            string indexDirPath = Directory.GetCurrentDirectory() + "\\index\\";
            return null;// new SjpLocalDiskIndex(Path.GetFullPath(".\\index\\"));
        }

        public static List<ISjpScraper> GetScrapers(ISjpStorage storage, ISjpIndex index)
        {
            List<ISjpScraper> scrapers = new List<ISjpScraper>();

            scrapers.Add(new Sjp2ndWatchScraper(storage, index));
            scrapers.Add(new SjpAdReadyScraper(storage, index));
            scrapers.Add(new SjpApptioScraper(storage, index));
            scrapers.Add(new SjpAnimotoScraper(storage, index));
            scrapers.Add(new SjpBizibleScraper(storage, index));
            scrapers.Add(new SjpBuuteeqScraper(storage, index));
            scrapers.Add(new SjpCheezburgerScraper(storage, index));
            scrapers.Add(new SjpContextRelevantScraper(storage, index));
            scrapers.Add(new SjpExtraHopScraper(storage, index));
            scrapers.Add(new SjpHaikuDeckScraper(storage, index));
            scrapers.Add(new SjpImpinjScraper(storage, index));
            scrapers.Add(new SjpIndochinoScraper(storage, index));
            scrapers.Add(new SjpIntrepidLearningScraper(storage, index));
            scrapers.Add(new SjpLumoScraper(storage, index));
            scrapers.Add(new SjpMozScraper(storage, index));
            scrapers.Add(new SjpMixpoScraper(storage, index));
            scrapers.Add(new SjpPayscaleScraper(storage, index));
            scrapers.Add(new SjpPlacedScraper(storage, index));
            scrapers.Add(new SjpQumuloScraper(storage, index));
            scrapers.Add(new SjpRedfinScraper(storage, index));
            scrapers.Add(new SjpRoverScraper(storage, index));
            scrapers.Add(new SjpSkytapScraper(storage, index));
            scrapers.Add(new SjpSnupiScraper(storage, index));
            scrapers.Add(new SjpSmartsheetScraper(storage, index));
            scrapers.Add(new SjpTier3Scraper(storage, index));
            scrapers.Add(new SjpUnionBayNetworksScraper(storage, index));
            scrapers.Add(new SjpWildTangentScraper(storage, index));
            scrapers.Add(new SjpZ2LiveScraper(storage, index));

            return scrapers;
        }
    }
}
