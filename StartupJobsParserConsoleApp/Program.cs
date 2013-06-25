﻿using StartupJobsParser;
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
            ISjpIndex index = new SjpLocalDiskIndex(Path.GetFullPath(".\\index\\"));

            List<ISjpScraper> scrapers = new List<ISjpScraper>();
            scrapers.Add(new Sjp2ndWatchScraper(".\\data\\2ndwatch\\", index));
            scrapers.Add(new SjpAdReadyScraper(".\\data\\adready\\", index));
            scrapers.Add(new SjpApptioScraper(".\\data\\apptio\\", index));
            scrapers.Add(new SjpAnimotoScraper(".\\data\\animoto\\", index));
            scrapers.Add(new SjpBuuteeqScraper(".\\data\\buuteeq\\", index));
            scrapers.Add(new SjpCheezburgerScraper(".\\data\\cheezburger\\", index));
            scrapers.Add(new SjpContextRelevantScraper(".\\data\\contextrelevant\\", index));
            scrapers.Add(new SjpExtraHopScraper(".\\data\\extrahop\\", index));
            scrapers.Add(new SjpHaikuDeckScraper(".\\data\\haikudeck\\", index));
            scrapers.Add(new SjpImpinjScraper(".\\data\\impinj\\", index));
            scrapers.Add(new SjpIndochinoScraper(".\\data\\indochino\\", index));
            scrapers.Add(new SjpIntrepidLearningScraper(".\\data\\intrepidlearning\\", index));
            scrapers.Add(new SjpMozScraper(".\\data\\moz\\", index));
            scrapers.Add(new SjpMixpoScraper(".\\data\\mixpo\\", index));
            scrapers.Add(new SjpPayscaleScraper(".\\data\\payscale\\", index));
            scrapers.Add(new SjpPlacedScraper(".\\data\\placed\\", index));
            scrapers.Add(new SjpQumuloScraper(".\\data\\qumulo\\", index));
            scrapers.Add(new SjpRedfinScraper(".\\data\\redfin\\", index));
            scrapers.Add(new SjpRoverScraper(".\\data\\rover\\", index));
            scrapers.Add(new SjpSkytapScraper(".\\data\\skytap\\", index));
            scrapers.Add(new SjpSmartsheetScraper(".\\data\\smartsheet\\", index));
            scrapers.Add(new SjpTier3Scraper(".\\data\\tier3\\", index));
            scrapers.Add(new SjpWildTangentScraper(".\\data\\wildtangent\\", index));
            scrapers.Add(new SjpZ2LiveScraper(".\\data\\z2live\\", index));

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
