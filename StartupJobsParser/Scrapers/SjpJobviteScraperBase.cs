using System;
using System.Net;
using HtmlAgilityPack;

namespace StartupJobsParser
{
    public abstract class SjpJobviteScraperBase : SjpScraper
    {
        private const string _listUriFormat = "http://hire.jobvite.com/CompanyJobs/Careers.aspx?c={0}&jvresize=";
        private const string _itemUriFormat = _listUriFormat + "&j={1}";

        protected abstract string JobviteCompanyId { get; }

        private Uri _defaultScrapeUri = null;
        public override Uri DefaultScrapeUri { get { return _defaultScrapeUri; } }

        public SjpJobviteScraperBase(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
            _defaultScrapeUri = new Uri(string.Format(_listUriFormat, JobviteCompanyId));
        }

        protected Uri GetItemUri(string jobId)
        {
            return new Uri(string.Format(_itemUriFormat, JobviteCompanyId, jobId));
        }

        protected Uri ExtractJdUriFromGoToPageLink(HtmlNode anchorNode)
        {
            HtmlAttribute attr = anchorNode.Attributes["onclick"];
            if (attr == null)
            {
                attr = anchorNode.Attributes["href"];
            }

            string jobRoute = attr.Value;
            int idStart = -1;
            int idEnd = jobRoute.LastIndexOf('\'');
            if (idEnd >= 0)
            {
                idStart = jobRoute.LastIndexOf('\'', idEnd - 1) + 1;
            }
            else
            {
                idStart = jobRoute.LastIndexOf("?jvi=") + 5;
                idEnd = jobRoute.IndexOf(',', idStart);
            }
            string jobId = jobRoute.Substring(idStart, idEnd - idStart) + ",Job";
            return GetItemUri(WebUtility.UrlEncode(jobId));
        }
    }
}