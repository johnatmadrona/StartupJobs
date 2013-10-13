using System.Threading.Tasks;

namespace StartupJobsParser
{
    public interface ISjpLinkTracker
    {
        string CreateTrackedLink(string url);
        Task<string> CreateTrackedLinkAsync(string url);
    }
}