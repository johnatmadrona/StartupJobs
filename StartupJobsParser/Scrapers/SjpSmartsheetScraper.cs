using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpSmartsheetScraper : SjpScraper
    {
        private static readonly Uri _defaultScrapeUri = new Uri("https://jobs.gild.com/widget/smartsheet.com");
        private static readonly Uri _publicUri = new Uri("https://www.smartsheet.com/careers");

        public override string CompanyName { get { return "Smartsheet"; } }
        public override Uri DefaultScrapeUri { get { return _defaultScrapeUri; } }
        public override Uri PublicUri { get { return _publicUri; } }

        public SjpSmartsheetScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            string text = SjpUtils.GetTextDoc(uri);

            const string startText = "widget.innerHTML = '";
            int start = text.IndexOf(startText);
            int end = -1;
            if (start >= 0)
            {
                start += startText.Length;
                for (int i = start + 1; i < text.Length; i++)
                {
                    if (text[i] == '\'' && text[i-1] != '\\')
                    {
                        end = i;
                        break;
                    }
                }
            }
            string html = text.Substring(start, end - start).Replace("\\'", "'");

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            HtmlNodeCollection headers = doc.DocumentNode.SelectNodes("//div[@class='row gild-job-header']");
            HtmlNodeCollection bodies = doc.DocumentNode.SelectNodes("//div[@class='row gild-job-body']");
            if (headers.Count != bodies.Count)
            {
                throw new Exception("Expected equal headers to bodies, but found " + headers.Count + " headers and " + bodies.Count + " bodies.");
            }

            for (int i = 0; i < headers.Count; i++)
            {
                yield return GetSmartsheetJd(headers[i], bodies[i]);
            }
        }

        private JobDescription GetSmartsheetJd(HtmlNode header, HtmlNode body)
        {
            HtmlNode titleNode = header.SelectSingleNode(".//*[@class='gild-job-position-title']");
            titleNode.SelectSingleNode("./small").Remove();

            string location = "Bellevue, WA";
            HtmlNode locationNode = header.SelectSingleNode(".//div[contains(@class,'gild-job-position-subtitle')]");
            string locationText = locationNode.InnerText;
            int locStart = locationText.IndexOf("&#9656;");
            int locEnd = locationText.IndexOf("&bull;");
            if (locStart >= 0)
            {
                locStart += 7;
                if (locEnd > locStart)
                {
                    string parsedLoc = locationText.Substring(locStart, locEnd - locStart).Trim();
                    if (parsedLoc.StartsWith("Location:"))
                    {
                        parsedLoc = parsedLoc.Substring(9).Trim();
                    }
                    if (parsedLoc.Length > 0)
                    {
                        location = parsedLoc;
                    }
                }
            }

            HtmlNode descriptionNode = body;
            descriptionNode.SelectSingleNode(".//*[@class='button_to']").ParentNode.Remove();

            return new JobDescription()
            {
                SourceUri = PublicUri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = location,
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}