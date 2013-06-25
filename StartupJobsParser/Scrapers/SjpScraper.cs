using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;

namespace StartupJobsParser
{
    public abstract class SjpScraper : ISjpScraper
    {
        protected string m_storageDirPath;
        protected ISjpIndex m_index;

        public abstract string CompanyName { get; }
        public abstract Uri DefaultUri { get; }

        // TODO: Abstract storage
        protected SjpScraper(string storageDirPath, ISjpIndex index)
        {
            // Use full path
            m_storageDirPath = Path.GetFullPath(storageDirPath);
            if (!Directory.Exists(m_storageDirPath))
            {
                Directory.CreateDirectory(m_storageDirPath);
            }

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

            List<string> removedJDs = new List<string>(Directory.GetFiles(m_storageDirPath));
            foreach (JobDescription jd in newJDs)
            {
                string filePath = m_storageDirPath + jd.Uid + ".jd";
                jd.StorageUri = filePath;

                if (!File.Exists(filePath))
                {
                    SjpLogger.Log("New JD: " + jd.Title);
                    using (FileStream file = File.Create(filePath))
                    {
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(JobDescription));
                        ser.WriteObject(file, jd);
                    }
                    m_index.AddToIndex(jd);
                }
                else
                {
                    // This is not removed, so take it off the removed list
                    removedJDs.Remove(filePath);
                }
            }

            foreach (string removedJD in removedJDs)
            {
                SjpLogger.Log("JD Removed: " + removedJD);
                string uid = Path.GetFileNameWithoutExtension(removedJD);
                m_index.RemoveFromIndex(uid);
                File.Delete(removedJD);
            }
        }

        protected abstract IEnumerable<JobDescription> GetJds(Uri uri);
    }
}