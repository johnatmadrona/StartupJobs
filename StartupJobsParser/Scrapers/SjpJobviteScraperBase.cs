using System;
using System.Net;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace StartupJobsParser
{
    public class SjpJobviteScraper : SjpScraper
    {
        protected const string _listUriFormat = "http://hire.jobvite.com/CompanyJobs/Careers.aspx?c={0}&jvresize=";
        protected const string _itemUriFormat = _listUriFormat + "&j={1}";

        private string _companyName;
        private string _jobviteCompanyId;
        private Uri _publicUri;
        private Uri _defaultScrapeUri;

        protected string JobviteCompanyId { get { return _jobviteCompanyId; } }

        public override string CompanyName { get { return _companyName; } }
        public override Uri PublicUri { get { return _publicUri; } }
        public override Uri DefaultScrapeUri { get { return _defaultScrapeUri; } }

        public SjpJobviteScraper(
            SjpScraperParams scraperParams,
            string companyName,
            string publicUri,
            string jobviteCompanyId
            )
            : this(scraperParams, companyName, publicUri, jobviteCompanyId, null)
        {
        }

        public SjpJobviteScraper(
            SjpScraperParams scraperParams,
            string companyName,
            string publicUri,
            string jobviteCompanyId,
            string scraperUri
            )
            : base(scraperParams)
        {
            _companyName = companyName;
            _publicUri = new Uri(publicUri);
            _jobviteCompanyId = jobviteCompanyId;
            if (scraperUri != null)
            {
                _defaultScrapeUri = new Uri(scraperUri);
            }
            else
            {
                _defaultScrapeUri = new Uri(string.Format(_listUriFormat, JobviteCompanyId));
            }
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

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            if (uri.Host.StartsWith("hire."))
            {
                return GetJdsFromHireSubdomain(uri);
            }
            else if (uri.Host.StartsWith("jobs."))
            {
                return GetJdsFromJobsSubdomain(uri);
            }
            else
            {
                throw new Exception("Unexpected subdomain in Jobvite URL: " + uri.AbsoluteUri);
            }
        }

        protected virtual IEnumerable<JobDescription> GetJdsFromHireSubdomain(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//*[@class='joblist']//a[starts-with(@onclick,'jvGoToPage')]");
            if (nodes == null)
            {
                nodes = doc.DocumentNode.SelectNodes("//*[@class='joblist']//a[contains(@href,'?jvi=')]");
            }
            if (nodes == null)
            {
                // XPath is case-sensitive - this handles the jobList (vs joblist) case
                nodes = doc.DocumentNode.SelectNodes("//*[@class='jobList']//a[contains(@href,'?jvi=')]");
            }

            foreach (HtmlNode jdUriNode in nodes)
            {
                yield return GetJdFromHireSubdomain(ExtractJdUriFromGoToPageLink(jdUriNode));
            }
        }

        protected virtual JobDescription GetJdFromHireSubdomain(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);

            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//div[@class='jvjobheader']/h2");

            HtmlNode locationNode = titleNode.NextSibling;
            string location = null;
            while (location == null)
            {
                if (locationNode.InnerText.Contains("|"))
                {
                    location = locationNode.InnerText.Split('|')[1];
                }
                else
                {
                    locationNode = locationNode.NextSibling;
                }
            }

            HtmlNode descriptionNode = doc.DocumentNode.SelectSingleNode("//div[@class='jvdescriptionbody']");

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = WebUtility.HtmlDecode(location).Trim(),
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }

        protected virtual IEnumerable<JobDescription> GetJdsFromJobsSubdomain(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//*[@class='jv-job-list-name']//a");
            foreach (HtmlNode jdUriNode in nodes)
            {
                Uri jobUri = new Uri(uri, jdUriNode.Attributes["href"].Value);
                yield return GetJdFromJobsSubdomain(jobUri);
            }
        }

        protected virtual JobDescription GetJdFromJobsSubdomain(Uri jdUri)
        {
            HtmlNode doc = SjpUtils.GetHtmlDoc(jdUri).DocumentNode;

            HtmlNode titleNode = doc.SelectSingleNode("//*[@class='jv-header']");

            HtmlNode locationNode = doc.SelectSingleNode("//*[@class='jv-job-detail-meta']");
            HtmlNode child = locationNode.FirstChild;
            while (child.Name != "span")
            {
                HtmlNode remove = child;
                child = child.NextSibling;
                remove.Remove();
            }
            child.Remove();
            string location = SjpUtils.GetCleanTextFromHtml(locationNode).Replace("\n", "");

            HtmlNode descriptionNode = doc.SelectSingleNode("//div[@class='jv-job-detail-description']");

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = location,
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}