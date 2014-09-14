using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace StartupJobsParser
{
    public abstract class SjpResumatorScraperBase : SjpScraper
    {
        protected SjpResumatorScraperBase(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            string text = null;

            WebRequest req = WebRequest.Create(uri);
            using (WebResponse res = req.GetResponse())
            {
                using (StreamReader reader = new StreamReader(res.GetResponseStream()))
                {
                    text = reader.ReadToEnd();
                }
            }

            const string SectionStartText = "class=\"resumator-job resumator-jobs-text\"";
            int offset = text.IndexOf(SectionStartText);
            offset = text.IndexOf('>', offset + SectionStartText.Length) + 1;

            int depth = 1;
            for (int i = offset; offset >= 0 && i < text.Length; i++)
            {
                if (text[i] == '<')
                {
                    string tagStart = text.Substring(i, 5);
                    if (tagStart.StartsWith("<div", StringComparison.OrdinalIgnoreCase))
                    {
                        depth++;
                    }
                    else if (tagStart.StartsWith("</div", StringComparison.OrdinalIgnoreCase))
                    {
                        depth--;
                        if (depth < 1)
                        {
                            HtmlDocument doc = new HtmlDocument();
                            doc.LoadHtml(text.Substring(offset, i - offset));
                            yield return GetResumatorJd(doc.DocumentNode);
                            offset = text.IndexOf(SectionStartText, i);
                        }
                    }
                }
            }
        }

        protected JobDescription GetResumatorJd(HtmlNode jdNode)
        {
            HtmlNode titleNode = jdNode.SelectSingleNode("div[starts-with(@class, 'resumator-job-title')]");

            HtmlNode locationNode = titleNode.NextSibling;
            const string prefix = "Location: ";
            const string dept = "Department:";
            string location = SjpUtils.GetCleanTextFromHtml(locationNode).Substring(prefix.Length);
            int deptOffset = location.IndexOf(dept);
            if (deptOffset >= 0)
            {
                location = location.Substring(0, deptOffset);
            }

            HtmlNode descriptionNode = 
                jdNode.SelectSingleNode("div/div[starts-with(@class, 'resumator-job-description')]");

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