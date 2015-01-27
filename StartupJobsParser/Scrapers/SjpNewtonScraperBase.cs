using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StartupJobsParser
{
    public abstract class SjpNewtonScraperBase : SjpScraper
    {
        private const string _listUriFormat = "http://newton.newtonsoftware.com/career/CareerHome.action?clientId={0}";
        private const string _itemUriFormat = _listUriFormat + "&id={1}";

        protected abstract string NewtonCompanyId { get; }

        private Uri _defaultScrapeUri = null;
        public override Uri DefaultScrapeUri { get { return _defaultScrapeUri; } }

        public SjpNewtonScraperBase(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
            _defaultScrapeUri = new Uri(string.Format(_listUriFormat, NewtonCompanyId));
        }

        protected Uri GetItemUri(string jobId)
        {
            return new Uri(string.Format(_itemUriFormat, NewtonCompanyId, jobId));
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//a[starts-with(@href, 'http://newton.newtonsoftware.com/career/JobIntroduction.action?')]"))
            {
                yield return GetJd(
                    SjpUtils.GetCleanTextFromHtml(jdLink),
                    new Uri(uri, jdLink.Attributes["href"].Value)
                    );
            }
        }

        protected abstract JobDescription GetJd(string title, Uri jdUri);
    }
}