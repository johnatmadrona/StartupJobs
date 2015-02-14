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
            SjpLogger.Log("Beginning run");

            string bitlyAccessToken = ConfigurationManager.AppSettings["BitlyToken"];
            string awsAccessKey = ConfigurationManager.AppSettings["AWSAccessKey"];
            string awsSecretKey = ConfigurationManager.AppSettings["AWSSecretKey"];

            if (string.IsNullOrEmpty(awsAccessKey) ||
                string.IsNullOrEmpty(awsSecretKey) ||
                string.IsNullOrEmpty(bitlyAccessToken))
            {
                SjpLogger.Log("ERROR: Must add values for AWSAccessKey, AWSSecretKey, and BitlyToken in application config");
                return;
            }

            SjpScraperParams scraperParams = new SjpScraperParams()
            {
                //Storage = new SjpStorageDisk(".\\"),
                Storage = (ISjpStorage)new SjpStorageS3(
                    awsAccessKey,
                    awsSecretKey,
                    RegionEndpoint.USWest2,
                    "madrona-sjp"
                    ),
                Index = GetIndex(),
                LinkTracker = null // new BitlyClient(bitlyAccessToken)
            };

            List<ISjpScraper> scrapers = GetScrapers(scraperParams);

            ScrapeResult aggregateScrapeResult = new ScrapeResult();

            List<KeyValuePair<Type, Exception>> errors = new List<KeyValuePair<Type,Exception>>();
            Parallel.ForEach(scrapers, scraper =>
            {
                try
                {
                    aggregateScrapeResult.Merge(scraper.Scrape());
                }
                catch (Exception ex)
                {
                    errors.Add(new KeyValuePair<Type,Exception>(scraper.GetType(), ex));
                }
            });

            try
            {
                string aggregateJdListKey = SjpScraper.StoragePathRoot + "aggregate";

                JobDescription[] allJds = aggregateScrapeResult.AllActiveJdsToArray();
                if (scraperParams.Storage.Exists(aggregateJdListKey))
                {
                    scraperParams.Storage.Delete(aggregateJdListKey);
                }
                scraperParams.Storage.Add(aggregateJdListKey, allJds.GetType(), allJds);
            }
            catch (Exception ex)
            {
                errors.Add(new KeyValuePair<Type, Exception>(
                    typeof(JobDescription[]),
                    new Exception(string.Format("Error while storing aggregate: {0}", ex), ex)
                    ));
            }

            if (errors.Count > 0)
            {
                SjpLogger.Log("\nERRORS:");
                SjpLogger.Log("=======\n");
                foreach (var error in errors)
                {
                    SjpLogger.Log("{0}:\n{1}\n", error.Key, error.Value);
                }
            }

            SjpLogger.Log("Run complete");
        }

        public static ISjpIndex GetIndex()
        {
            string indexDirPath = Directory.GetCurrentDirectory() + "\\index\\";
            return null;// new SjpLocalDiskIndex(Path.GetFullPath(".\\index\\"));
        }

        public static List<ISjpScraper> GetScrapers(SjpScraperParams sp)
        {
            List<ISjpScraper> scrapers = new List<ISjpScraper>();

            scrapers.Add(new SjpResumatorScraper(sp, "2nd Watch", "http://2ndwatch.com/contact-us/careers/", "2ndwatch"));
            scrapers.Add(new SjpAngelListScraper(sp, "Algorithmia", "https://angel.co/algorithmia/jobs"));
            scrapers.Add(new SjpApptioScraper(sp));
            scrapers.Add(new SjpAnimotoScraper(sp));
            scrapers.Add(new SjpBizibleScraper(sp));
            scrapers.Add(new SjpBoomerangCommerceScraper(sp));
            scrapers.Add(new SjpResumatorScraper(sp, "Cheezburger", "http://jobs.cheezburger.com/", "cheezburger"));
            scrapers.Add(new SjpContextRelevantScraper(sp));
            scrapers.Add(new SjpEvocalizeScraper(sp));
            scrapers.Add(new SjpExtraHopScraper(sp));
            scrapers.Add(new SjpHaikuDeckScraper(sp));
            scrapers.Add(new SjpHighspotScraper(sp));
            scrapers.Add(new SjpAngelListScraper(sp, "Icebrg", "https://angel.co/icebrg-io/jobs"));
            scrapers.Add(new SjpIgneousSystemsScraper(sp));
            scrapers.Add(new SjpImpinjScraper(sp));
            scrapers.Add(new SjpIndochinoScraper(sp));
            scrapers.Add(new SjpIntrepidLearningScraper(sp));
            scrapers.Add(new SjpISpotTvScraper(sp));
            scrapers.Add(new SjpJamaScraper(sp));
            scrapers.Add(new SjpResumatorScraper(sp, "Jobaline", "http://jobalinecareers.theresumator.com/", "jobalinecareers"));
            scrapers.Add(new SjpResumatorScraper(sp, "Julep", "http://www.julep.com/careers.html", "julepbeauty"));
            scrapers.Add(new SjpLumoScraper(sp));
            scrapers.Add(new SjpMaxPointScraper(sp));
            scrapers.Add(new SjpMercentScraper(sp));
            scrapers.Add(new SjpResumatorScraper(sp, "Moz", "http://moz.com/about/jobs", "moz"));
            scrapers.Add(new SjpMixpoScraper(sp));
            scrapers.Add(new SjpOpalScraper(sp));
            scrapers.Add(new SjpPeachScraper(sp));
            scrapers.Add(new SjpPlacedScraper(sp));
            scrapers.Add(new SjpResumatorScraper(sp, "Wonder Workshop", "https://www.makewonder.com/careers", "Playi"));
            scrapers.Add(new SjpQumuloScraper(sp));
            scrapers.Add(new SjpRedfinScraper(sp));
            scrapers.Add(new SjpResolutionTubeScraper(sp));
            scrapers.Add(new SjpResumatorScraper(sp, "Rover.com", "http://jobs.rover.com/", "rover"));
            scrapers.Add(new SjpSeeqScraper(sp));
            scrapers.Add(new SjpSkytapScraper(sp));
            scrapers.Add(new SjpResumatorScraper(sp, "SNUPI", "http://www.snupi.com/#jobs", "snupi"));
            scrapers.Add(new SjpSmartsheetScraper(sp));
            scrapers.Add(new SjpAngelListScraper(sp, "Spare5", "https://angel.co/spare5/jobs"));
            scrapers.Add(new SjpWildTangentScraper(sp));
            scrapers.Add(new SjpYieldexScraper(sp));
            scrapers.Add(new SjpZ2Scraper(sp));

            return scrapers;
        }
    }
}
