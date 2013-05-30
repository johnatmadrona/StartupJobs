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
        public abstract Uri PublicUri { get; }

        protected SjpResumatorScraperBase(string storageDirPath, ISjpIndex index)
            : base(storageDirPath, index)
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
                            yield return GetResumatorJd(doc.DocumentNode, PublicUri);
                            offset = text.IndexOf(SectionStartText, i);
                        }
                    }
                }
            }
        }

        protected JobDescription GetResumatorJd(HtmlNode jdNode, Uri uri)
        {
            HtmlNode titleNode = jdNode.SelectSingleNode("div[starts-with(@class, 'resumator-job-title')]");
            string title = titleNode.InnerText;

            HtmlNode locationNode = titleNode.NextSibling;
            Regex regex = new Regex(@"(?<Location>\w+, [A-Z]{2})");
            string location = regex.Match(locationNode.InnerText).Groups["Location"].Value;

            string description = jdNode.SelectSingleNode("div/div[starts-with(@class, 'resumator-job-description')]").InnerHtml;

            return new JobDescription()
            {
                SourceUri = uri.AbsoluteUri,
                Company = CompanyName,
                Title = WebUtility.HtmlDecode(titleNode.InnerText).Trim(),
                Location = WebUtility.HtmlDecode(location).Trim(),
                FullTextDescription = WebUtility.HtmlDecode(description).Trim(),
                FullHtmlDescription = description
            };
        }
    }
}