using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;

namespace StartupJobsParser
{
    public abstract class SjpScraper : ISjpScraper
    {
        protected ISjpStorage m_storage;
        protected ISjpIndex m_index;

        public abstract string CompanyName { get; }
        public abstract Uri DefaultUri { get; }

        protected SjpScraper(ISjpStorage storage, ISjpIndex index)
        {
            if (storage == null)
            {
                throw new ArgumentNullException("storage", "Storage cannot be null");
            }
            m_storage = storage;
            m_index = index;
        }

        public void Scrape()
        {
            Scrape(DefaultUri);
        }

        public void Scrape(Uri uri)
        {
            List<JobDescription> newJDs;
            try
            {
                newJDs = new List<JobDescription>(GetJds(uri));
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

            List<string> removedJDs = new List<string>(m_storage.List());
            foreach (JobDescription jd in newJDs)
            {
                string id = jd.Uid + ".jd";
                if (!m_storage.Exists(id))
                {
                    SjpLogger.Log("New JD: " + jd.Title);
                    m_storage.Add(id, typeof(JobDescription), jd);
                    if (m_index != null)
                    {
                        m_index.AddToIndex(jd);
                    }
                }
                else
                {
                    // This is not removed, so take it off the removed list
                    removedJDs.Remove(id);
                }
            }

            foreach (string removedJD in removedJDs)
            {
                SjpLogger.Log("JD Removed: " + removedJD);
                string uid = Path.GetFileNameWithoutExtension(removedJD);
                if (m_index != null)
                {
                    m_index.RemoveFromIndex(uid);
                }
                m_storage.Delete(removedJD);
            }
        }

        protected abstract IEnumerable<JobDescription> GetJds(Uri uri);
    }
}