using System.Collections.Generic;

namespace StartupJobsParser
{
    public interface ISjpIndex
    {
        void AddToIndex(JobDescription jd);
        void RemoveFromIndex(string uid);
        IEnumerable<JobDescription> FindJds(string term);
    }
}