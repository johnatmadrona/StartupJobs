using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;

namespace StartupJobsParser
{
    // TODO: Separate scraping from storing and indexing
    public abstract class SjpScraper : ISjpScraper
    {
        protected ISjpStorage _storage;
        protected ISjpIndex _index;
        protected ISjpLinkTracker _linkTracker;

        public abstract string CompanyName { get; }
        public abstract Uri DefaultScrapeUri { get; }
        public abstract Uri PublicUri { get; }

        public static string StoragePathRoot { get { return "data/"; } }

        protected SjpScraper(SjpScraperParams scraperParams)
        {
            if (scraperParams == null)
            {
                throw new ArgumentNullException("Must provide params");
            }
            else if (scraperParams.Storage == null)
            {
                throw new ArgumentNullException("Must provide a storage object");
            }
            _storage = scraperParams.Storage;
            _index = scraperParams.Index;
            _linkTracker = scraperParams.LinkTracker;
        }

        public ScrapeResult Scrape()
        {
            return Scrape(DefaultScrapeUri);
        }

        public ScrapeResult Scrape(Uri uri)
        {
            List<JobDescription> newJds;
            try
            {
                newJds = new List<JobDescription>(GetJds(uri));
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpStatusCode httpStatusCode = ((HttpWebResponse)ex.Response).StatusCode;
                    if (httpStatusCode == HttpStatusCode.InternalServerError)
                    {
                        // If internal error, just skip this and retrieve 
                        // the data in a future run
                        SjpLogger.Log("WARNING: Skipping '{0}' scraper due to remote server error", CompanyName);
                        return null;
                    }
                    else
                    {
                        SjpLogger.Log("ERROR: Http request status {0}. {1}", httpStatusCode, ex);
                    }
                }
                else
                {
                    SjpLogger.Log("ERROR: Web request status {0}. {1}", ex.Status, ex);
                }

                throw;
            }

            ScrapeResult result = CreateScrapeResult(newJds);
            RemoveObsoleteJds(result.ObsoleteJdIds);

            return result;
        }

        private ScrapeResult CreateScrapeResult(List<JobDescription> jds)
        {
            ScrapeResult result = new ScrapeResult();

            // Create a list of previously discovered JDs to identify obsolete JDs
            List<string> obsoleteJds = new List<string>(_storage.List(GetJdStoragePathPrefix()));


            // Add new JDs to storage and refine the list of obsolete JDs
            foreach (JobDescription jd in jds)
            {
                string key = GetJdStorageKey(jd);
                if (!_storage.Exists(key))
                {
                    // This is an item we haven't seen before
                    SjpLogger.Log("New JD: " + jd.Title);
                    result.NewJds.Add(jd);

                    _storage.Add(key, typeof(JobDescription), jd);
                    if (_index != null)
                    {
                        _index.AddToIndex(jd);
                    }
                }
                else
                {
                    // This item already exists in storage
                    result.OldJds.Add(jd);

                    // Remove it from the obsolete list
                    obsoleteJds.Remove(key);
                }
            }

            // Anything in storage that was not found with the current 
            // JDs is obsolete
            result.ObsoleteJdIds = obsoleteJds;

            return result;
        }

        private void RemoveObsoleteJds(List<string> obsoleteJdIds)
        {
            foreach (string obsoleteJdId in obsoleteJdIds)
            {
                SjpLogger.Log("JD Removed: " + obsoleteJdId);
                if (_index != null)
                {
                    _index.RemoveFromIndex(obsoleteJdId);
                }
                _storage.Delete(obsoleteJdId);
            }
        }

        public string GetJdStoragePathPrefix()
        {
            Regex badChars = new Regex(@"[^a-z^0-9^\.^-]+");
            return StoragePathRoot + badChars.Replace(CompanyName.ToLowerInvariant(), "") + "/";
        }

        public string GetJdStorageKey(JobDescription jd)
        {
            string uid = ((uint)jd.ToString().GetHashCode()).ToString();
            return GetJdStoragePathPrefix() + uid + ".jd";
        }

        protected abstract IEnumerable<JobDescription> GetJds(Uri uri);

        protected string TryCreateTrackedLink(Uri uri)
        {
            if (_linkTracker != null)
            {
                // TODO: Handle expetions
                return _linkTracker.CreateTrackedLink(uri.AbsoluteUri);
            }

            return uri.AbsoluteUri;
        }
    }
}