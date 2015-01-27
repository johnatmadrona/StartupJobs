using HtmlAgilityPack;
using System;

namespace StartupJobsParser
{
    public class SjpJamaScraper : SjpNewtonScraperBase
    {
        private Uri _publicUri = new Uri("http://www.jamasoftware.com/careers/");
        public override string CompanyName { get { return "Jama"; } }
        public override Uri PublicUri { get { return _publicUri; } }

        protected override string NewtonCompanyId { get { return "8a8725d048f86d3101490ba21f5d3d16"; } }

        public SjpJamaScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override JobDescription GetJd(string title, Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);
            string location = "Portland, OR";
            HtmlNode descriptionNode = doc.DocumentNode.SelectSingleNode("//td[@id='gnewtonJobDescriptionText']");

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = title,
                Location = location,
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}