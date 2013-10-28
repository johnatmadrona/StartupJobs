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
            string bitlyAccessToken = ConfigurationManager.AppSettings["BitlyToken"];
            string awsAccessKey = ConfigurationManager.AppSettings["AWSAccessKey"];
            string awsSecretKey = ConfigurationManager.AppSettings["AWSSecretKey"];

            if (string.IsNullOrEmpty(awsAccessKey) ||
                string.IsNullOrEmpty(awsSecretKey) ||
                string.IsNullOrEmpty(bitlyAccessToken))
            {
                Console.WriteLine("ERROR: Must add values for AWSAccessKey, AWSSecretKey, and BitlyToken in application config");
                return;
            }

            SjpScraperParams scraperParams = new SjpScraperParams()
            {
                //SjpStorageDisk storage = new SjpStorageDisk(".\\data\\");
                Storage = (ISjpStorage)new SjpStorageS3(
                    awsAccessKey,
                    awsSecretKey,
                    RegionEndpoint.USWest2,
                    "madrona-sjp"
                    ),
                Index = GetIndex(),
                LinkTracker = new BitlyClient(bitlyAccessToken)
            };

            List<ISjpScraper> scrapers = GetScrapers(scraperParams);

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

        public static List<ISjpScraper> GetScrapers(SjpScraperParams scraperParams)
        {
            List<ISjpScraper> scrapers = new List<ISjpScraper>();

            scrapers.Add(new Sjp2ndWatchScraper(scraperParams));
            scrapers.Add(new SjpApptioScraper(scraperParams));
            scrapers.Add(new SjpAnimotoScraper(scraperParams));
            scrapers.Add(new SjpBizibleScraper(scraperParams));
            scrapers.Add(new SjpBuuteeqScraper(scraperParams));
            scrapers.Add(new SjpCheezburgerScraper(scraperParams));
            scrapers.Add(new SjpContextRelevantScraper(scraperParams));
            scrapers.Add(new SjpExtraHopScraper(scraperParams));
            scrapers.Add(new SjpHaikuDeckScraper(scraperParams));
            scrapers.Add(new SjpImpinjScraper(scraperParams));
            scrapers.Add(new SjpIndochinoScraper(scraperParams));
            scrapers.Add(new SjpIntrepidLearningScraper(scraperParams));
            scrapers.Add(new SjpJamaScraper(scraperParams));
            scrapers.Add(new SjpJobalineScraper(scraperParams));
            scrapers.Add(new SjpLumoScraper(scraperParams));
            scrapers.Add(new SjpMozScraper(scraperParams));
            scrapers.Add(new SjpMixpoScraper(scraperParams));
            scrapers.Add(new SjpPayscaleScraper(scraperParams));
            scrapers.Add(new SjpPlacedScraper(scraperParams));
            scrapers.Add(new SjpQumuloScraper(scraperParams));
            scrapers.Add(new SjpRedfinScraper(scraperParams));
            scrapers.Add(new SjpRoverScraper(scraperParams));
            scrapers.Add(new SjpSeeqScraper(scraperParams));
            scrapers.Add(new SjpSkytapScraper(scraperParams));
            scrapers.Add(new SjpSnupiScraper(scraperParams));
            scrapers.Add(new SjpSmartsheetScraper(scraperParams));
            scrapers.Add(new SjpTier3Scraper(scraperParams));
            scrapers.Add(new SjpUnionBayNetworksScraper(scraperParams));
            scrapers.Add(new SjpWildTangentScraper(scraperParams));
            scrapers.Add(new SjpZ2LiveScraper(scraperParams));

            return scrapers;
        }
    }
}
