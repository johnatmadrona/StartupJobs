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

        private Uri _defaultUri = null;
        public override Uri DefaultUri { get { return _defaultUri; } }

        public SjpJobviteScraperBase(string storageDirPath, ISjpIndex index)
            : base(storageDirPath, index)
        {
            _defaultUri = new Uri(string.Format(_listUriFormat, JobviteCompanyId));
        }

        protected Uri GetItemUri(string jobId)
        {
            return new Uri(string.Format(_itemUriFormat, JobviteCompanyId, jobId));
        }

        protected Uri ExtractJdUriFromGoToPageLink(HtmlNode anchorNode)
        {
            string jobRoute = anchorNode.Attributes["onclick"].Value;
            int idEnd = jobRoute.LastIndexOf('\'');
            int idStart = jobRoute.LastIndexOf('\'', idEnd - 1) + 1;
            string jobId = jobRoute.Substring(idStart, idEnd - idStart) + ",Job";
            return GetItemUri(WebUtility.UrlEncode(jobId));
        }
    }
}