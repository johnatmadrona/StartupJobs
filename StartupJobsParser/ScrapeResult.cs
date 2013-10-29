using System.Collections.Generic;

namespace StartupJobsParser
{
    /// <summary>
    /// Results of job description scraping. JDs are separated into 
    /// new JDs (one not seen before), old JDs (one seen before), and 
    /// obsolete JDs (one seen before, but no longer active).
    /// In general, this class is NOT thread-safe.
    /// </summary>
    public class ScrapeResult
    {
        private List<JobDescription> _newJds = null;
        private List<JobDescription> _oldJds = null;
        private List<string> _obsoleteJdIds = null;

        public List<JobDescription> NewJds
        {
            get
            {
                // Don't lock - we'll point to one version or the other
                if (_newJds == null) _newJds = new List<JobDescription>();
                return _newJds;
            }
            set { _newJds = value; }
        }

        public List<JobDescription> OldJds
        {
            get
            {
                if (_oldJds == null) _oldJds = new List<JobDescription>();
                return _oldJds;
            }
            set { _oldJds = value; }
        }

        public List<string> ObsoleteJdIds
        {
            get
            {
                if (_obsoleteJdIds == null) _obsoleteJdIds = new List<string>();
                return _obsoleteJdIds;
            }
            set { _obsoleteJdIds = value; }
        }

        public IEnumerable<JobDescription> AllActiveJds()
        {
            foreach (JobDescription jd in NewJds)
            {
                yield return jd;
            }
            foreach (JobDescription jd in OldJds)
            {
                yield return jd;
            }
        }

        public JobDescription[] AllActiveJdsToArray()
        {
            JobDescription[] jds = new JobDescription[NewJds.Count + OldJds.Count];

            int i = 0;
            foreach (JobDescription jd in AllActiveJds())
            {
                jds[i] = jd;
                i++;
            }

            return jds;
        }

        /// <summary>
        /// Copies items from 'other' to this. In effect, this is 
        /// a deep copy of the content of 'other'. Merging with self 
        /// is allowed, resulting in a doubling of object contents.
        /// </summary>
        /// <param name="other">ScrapeResult to copy items from.</param>
        public void Merge(ScrapeResult other)
        {
            foreach (JobDescription jd in other.NewJds)
            {
                NewJds.Add(jd);
            }
            foreach (JobDescription jd in other.OldJds)
            {
                OldJds.Add(jd);
            }
            foreach (string id in other.ObsoleteJdIds)
            {
                ObsoleteJdIds.Add(id);
            }
        }
    }
}