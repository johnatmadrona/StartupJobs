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

            List<KeyValuePair<string, Exception>> errors = new List<KeyValuePair<string, Exception>>();
            Parallel.ForEach(scrapers, scraper =>
            {
                try
                {
                    aggregateScrapeResult.Merge(scraper.Scrape());
                }
                catch (Exception ex)
                {
                    errors.Add(new KeyValuePair<string, Exception>(
                        scraper.GetType().ToString() + ": " + scraper.ScraperId,
                        ex
                        ));
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
                errors.Add(new KeyValuePair<string, Exception>(
                    "jd-aggregation",
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
            scrapers.Add(new SjpJobviteScraper(sp, "Animoto", "http://animoto.com/about/careers", "qBr9VfwQ"));
            scrapers.Add(new SjpBizibleScraper(sp));
            scrapers.Add(new SjpJobviteScraper(sp, "Boomerang Commerce", "http://www.boomerangcommerce.com/careers/", "qg0aVfw5"));
            scrapers.Add(new SjpResumatorScraper(sp, "Cheezburger", "http://jobs.cheezburger.com/", "cheezburger"));
            scrapers.Add(new SjpResumatorScraper(sp, "Context Relevant", "http://www.contextrelevant.com/job-openings/", "contextrelevant"));
            scrapers.Add(new SjpJobviteScraper(sp, "Dato", "http://dato.com/company/careers/index.html", "qY5aVfwS", "http://jobs.jobvite.com/careers/dato/jobs"));
            scrapers.Add(new SjpEchodyneScraper(sp));
            scrapers.Add(new SjpEvocalizeScraper(sp));
            scrapers.Add(new SjpJobviteScraper(sp, "ExtraHop", "http://www.extrahop.com/company/jobs/", "qAYaVfwn"));
            scrapers.Add(new SjpHaikuDeckScraper(sp));
            scrapers.Add(new SjpHighspotScraper(sp));
            scrapers.Add(new SjpAngelListScraper(sp, "Icebrg", "https://angel.co/icebrg-io/jobs"));
            scrapers.Add(new SjpJobviteScraper(sp, "Igneous Systems", "http://hire.jobvite.com/CompanyJobs/Careers.aspx?k=JobListing&c=qpO9Vfw1&v=1", "qpO9Vfw1"));
            scrapers.Add(new SjpImpinjScraper(sp));
            scrapers.Add(new SjpIndochinoScraper(sp));
            scrapers.Add(new SjpJobviteScraper(sp, "iSpot.tv", "http://www.ispot.tv/careers", "qrI9VfwX"));
            scrapers.Add(new SjpJamaScraper(sp));
            scrapers.Add(new SjpJobalineScraper(sp));
            scrapers.Add(new SjpResumatorScraper(sp, "Julep", "http://www.julep.com/careers.html", "julepbeauty"));
            scrapers.Add(new SjpLumoScraper(sp));
            scrapers.Add(new SjpMaxPointScraper(sp));
            scrapers.Add(new SjpResumatorScraper(sp, "Moz", "http://moz.com/about/jobs", "moz"));
            scrapers.Add(new SjpJobviteScraper(sp, "Mixpo", "http://dynamicvideoad.mixpo.com/about/careers/", "qc3aVfw4"));
            scrapers.Add(new SjpOpalScraper(sp));
            scrapers.Add(new SjpPeachScraper(sp));
            scrapers.Add(new SjpGreenhouseScraper(sp, "Playfab", "https://playfab.com/jobs", "playfab"));
            scrapers.Add(new SjpResumatorScraper(sp, "Placed", "http://www.placed.com/about/careers", "placed"));
            scrapers.Add(new SjpJobviteScraper(sp, "Pro.com", "http://hire.jobvite.com/CompanyJobs/Careers.aspx?k=JobListing&c=qxYaVfwk&v=1", "qxYaVfwk"));
            scrapers.Add(new SjpResumatorScraper(sp, "Wonder Workshop", "https://www.makewonder.com/careers", "Playi"));
            scrapers.Add(new SjpGreenhouseScraper(sp, "Qumulo", "http://qumulo.com/people/jobs/", "qumulo"));
            scrapers.Add(new SjpRedfinScraper(sp));
            scrapers.Add(new SjpResolutionTubeScraper(sp));
            scrapers.Add(new SjpResumatorScraper(sp, "Rover.com", "http://jobs.rover.com/", "rover"));
            scrapers.Add(new SjpSeeqScraper(sp));
            scrapers.Add(new SjpResumatorScraper(sp, "Shippable", "http://shippable.theresumator.com/", "shippable"));
            scrapers.Add(new SjpJobviteScraper(sp, "Skytap", "http://www.skytap.com/company/careers", "q1A9Vfwp"));
            scrapers.Add(new SjpResumatorScraper(sp, "SNUPI", "http://www.snupi.com/#jobs", "snupi"));
            scrapers.Add(new SjpSmartsheetScraper(sp));
            scrapers.Add(new SjpSpare5Scraper(sp));
            scrapers.Add(new SjpWildTangentScraper(sp));

            return scrapers;
        }
    }
}
