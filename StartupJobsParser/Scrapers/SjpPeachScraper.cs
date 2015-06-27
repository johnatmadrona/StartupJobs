using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace StartupJobsParser
{
    internal class PeachJob
    {
        JContainer _jc;

        public string Title { get {
            return _jc.Value<string>("title");
        } }

        public string DescriptionHtml { get {
            return ToHtml("Description", _jc.SelectToken("description"));
        } }

        public string ResponsibilitiesHtml { get {
            return ToHtml("Responsibilities", _jc.SelectToken("responsibilities"));
        } }

        public string RequirementsHtml { get {
            return ToHtml("Requirements", _jc.SelectToken("requirements"));
        } }

        public string BonusesHtml { get {
            return ToHtml("Bonuses", _jc.SelectToken("bonuses"));
        } }

        public PeachJob(JContainer jc)
        {
            _jc = jc;
        }

        private string ToHtml(string name, JToken t)
        {
            if (t == null)
            {
                return "";
            }

            List<string> parts = new List<string>();
            if (t.Type == JTokenType.String)
            {
                parts.Add(t.Value<string>());
            }
            else if (t.Type == JTokenType.Array)
            {
                parts.AddRange(t.Values<string>());
            }

            if (parts.Count < 1)
            {
                return "";
            }

            string html = "<div><h2>" + name + "</h2>";
            if (parts.Count == 1)
            {
                html += parts[0];
            }
            else
            {
                html += "<ul><li>";
                html += string.Join("</li><li>", parts);
                html += "</li></ul>";
            }
            html += "</div>";

            return html;
        }
    }

    public class SjpPeachScraper : SjpScraper
    {
        private static readonly Uri _publicUri = new Uri("https://www.peachd.com/jobs");
        private const string _defaultLocation = "Seattle, WA";

        public override string CompanyName { get { return "Peach"; } }
        public override Uri DefaultScrapeUri { get { return _publicUri; } }
        public override Uri PublicUri { get { return _publicUri; } }

        public SjpPeachScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);

            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//script[starts-with(@src,'https://peach-us.s3.amazonaws.com/prod/js/')]"))
            {
                foreach (JobDescription jd in GetJdsFromJs(node.Attributes["src"].Value))
                {
                    yield return jd;
                }
            }
        }

        private IEnumerable<JobDescription> GetJdsFromJs(string jsUri)
        {
            string doc = SjpUtils.GetTextDoc(jsUri);

            const string startText = "new Job(";
            int offset = doc.IndexOf(startText);
            while (offset >= 0)
            {
                // Find opening of json object
                offset += startText.Length;
                while (offset < doc.Length && doc[offset] != '{')
                {
                    offset++;
                }

                if (offset < doc.Length)
                {
                    // Find end of json object
                    int depth = 1;
                    int end;
                    for (end = offset + 1; end < doc.Length && depth > 0; end++)
                    {
                        if (doc[end] == '{') depth++;
                        else if (doc[end] == '}') depth--;
                    }

                    string json = doc.Substring(offset, end - offset);
                    yield return CreatePeachJd(json);

                    offset = doc.IndexOf(startText, end);
                }
            }
        }

        private JobDescription CreatePeachJd(string json)
        {
            JContainer jc = (JContainer)JsonConvert.DeserializeObject(json);
            PeachJob job = new PeachJob(jc);

            string htmlDescription = job.DescriptionHtml + 
                job.ResponsibilitiesHtml + 
                job.RequirementsHtml + 
                job.BonusesHtml;

            return new JobDescription()
            {
                SourceUri = PublicUri.AbsoluteUri,
                Company = CompanyName,
                Title = job.Title,
                Location = _defaultLocation,
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(htmlDescription),
                FullHtmlDescription = htmlDescription
            };
        }
    }
}