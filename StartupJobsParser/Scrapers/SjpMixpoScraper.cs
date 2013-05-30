using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpMixpoScraper : SjpScraper
    {
        private Uri _defaultUri = new Uri("http://dynamicvideoad.mixpo.com/about/careers/");
        public override string CompanyName { get { return "Mixpo"; } }
        public override Uri DefaultUri
        {
            get { return _defaultUri; }
        }

        public SjpMixpoScraper(string storageDirPath, ISjpIndex index)
            : base(storageDirPath, index)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdLink in doc.DocumentNode.SelectNodes("//a[starts-with(@href, '/about/career_position/')]"))
            {
                yield return GetMixpoJd(new Uri(uri, jdLink.Attributes["href"].Value));
            }
        }

        private JobDescription GetMixpoJd(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);
            HtmlNode jdNode = doc.DocumentNode.SelectSingleNode("//div[@id='content']");
            
            string title = jdNode.SelectSingleNode("h2").InnerText;

            HtmlNode remove = jdNode.SelectSingleNode("p[@id='breadcrumb']");
            remove.ParentNode.RemoveChild(remove, false);
            remove = jdNode.SelectSingleNode("h2");
            remove.ParentNode.RemoveChild(remove, false);
            remove = jdNode.SelectSingleNode("a[starts-with(@class, 'email')]");
            remove.ParentNode.RemoveChild(remove, false);

            string description = jdNode.InnerHtml;

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = WebUtility.HtmlDecode(title).Trim(),
                Location = "Seattle, WA",
                FullTextDescription = WebUtility.HtmlDecode(description).Trim(),
                FullHtmlDescription = description
            };
        }
    }
}