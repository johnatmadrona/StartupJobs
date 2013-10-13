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
        protected ISjpStorage m_storage;
        protected ISjpIndex m_index;

        public abstract string CompanyName { get; }
        public abstract Uri DefaultUri { get; }

        protected SjpScraper(SjpScraperParams scraperParams)
        {
            if (scraperParams == null)
            {
                throw new ArgumentNullException("storage", "Storage cannot be null");
            }
            m_storage = scraperParams.Storage;
            m_index = scraperParams.Index;
        }

        public void Scrape()
        {
            Scrape(DefaultUri);
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
                if (m_index != null)
                {
                    m_index.RemoveFromIndex(obsoleteJd);
                }
                m_storage.Delete(obsoleteJd);
            }
        }

        private List<string> StoreNewJdsAndGetObsoleteJds(List<JobDescription> newJds)
        {
            // Create a list of previously discovered JDs to identify obsolete JDs
            List<string> obsoleteJds = new List<string>(m_storage.List(GetJdStoragePrefix()));


            // Add new JDs to storage and refine the list of obsolete JDs
            foreach (JobDescription jd in newJds)
            {
                string key = GetJdStorageKey(jd);
                if (!m_storage.Exists(key))
                {
                    SjpLogger.Log("New JD: " + jd.Title);
                    m_storage.Add(key, typeof(JobDescription), jd);
                    if (m_index != null)
                    {
                        m_index.AddToIndex(jd);
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
                if (m_index != null)
                {
                    m_index.RemoveFromIndex(uid);
                }
                m_storage.Delete(obsoleteJd);
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
    }
}