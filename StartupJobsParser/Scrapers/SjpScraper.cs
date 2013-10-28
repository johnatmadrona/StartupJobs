using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;

namespace StartupJobsParser
{
    public abstract class SjpScraper : ISjpScraper
    {
        protected ISjpStorage _storage;
        protected ISjpIndex _index;
        protected ISjpLinkTracker _linkTracker;

        public string UriTag { get { return "?rs=sjp"; } }
        public abstract string CompanyName { get; }
        public abstract Uri DefaultScrapeUri { get; }
        public abstract Uri PublicUri { get; }

        public Uri PublicTaggedUri
        {
            get
            {
                return new Uri(PublicUri.AbsoluteUri + UriTag);
            }
        }

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

        public void Scrape()
        {
            Scrape(DefaultScrapeUri);
        }

        public void Scrape(Uri uri)
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
                        Console.WriteLine("WARNING: Skipping '{0}' scraper due to remote server error", CompanyName);
                        return;
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Http request status {0}. {1}", httpStatusCode, ex);
                    }
                }
                else
                {
                    Console.WriteLine("ERROR: Web request status {0}. {1}", ex.Status, ex);
                }

                throw;
            }

            List<string> obsoleteJds = StoreNewJdsAndGetObsoleteJds(newJds);

            foreach (string obsoleteJd in obsoleteJds)
            {
                SjpLogger.Log("JD Removed: " + obsoleteJd);
                if (_index != null)
                {
                    _index.RemoveFromIndex(obsoleteJd);
                }
                _storage.Delete(obsoleteJd);
            }
        }

        private List<string> StoreNewJdsAndGetObsoleteJds(List<JobDescription> newJds)
        {
            // Create a list of previously discovered JDs to identify obsolete JDs
            List<string> obsoleteJds = new List<string>(_storage.List(GetJdStoragePrefix()));


            // Add new JDs to storage and refine the list of obsolete JDs
            foreach (JobDescription jd in newJds)
            {
                string key = GetJdStorageKey(jd);
                if (!_storage.Exists(key))
                {
                    SjpLogger.Log("New JD: " + jd.Title);
                    _storage.Add(key, typeof(JobDescription), jd);
                    if (_index != null)
                    {
                        _index.AddToIndex(jd);
                    }
                }
                else
                {
                    // This item already exists in storage, meaning it's 
                    // on the obsolete list, but shouldn't be
                    obsoleteJds.Remove(key);
                }
            }

            return obsoleteJds;
        }

        private void RemoveObsoleteJds(List<string> obsoleteJds)
        {
            foreach (string obsoleteJd in obsoleteJds)
            {
                SjpLogger.Log("JD Removed: " + obsoleteJd);
                string uid = Path.GetFileNameWithoutExtension(obsoleteJd);
                if (_index != null)
                {
                    _index.RemoveFromIndex(uid);
                }
                _storage.Delete(obsoleteJd);
            }
        }

        public string GetJdStoragePrefix()
        {
            Regex badChars = new Regex(@"[^a-z^0-9^\.^-]+");
            return "data/" + badChars.Replace(CompanyName.ToLowerInvariant(), "") + "/";
        }

        public string GetJdStorageKey(JobDescription jd)
        {
            string uid = ((uint)jd.ToString().GetHashCode()).ToString();
            return GetJdStoragePrefix() + uid + ".jd";
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