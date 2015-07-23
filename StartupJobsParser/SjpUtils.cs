using HtmlAgilityPack;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace StartupJobsParser
{
    public static class SjpUtils
    {
        public static HtmlDocument GetHtmlDoc(string uri)
        {
            return GetHtmlDoc(new Uri(uri));
        }

        public static HtmlDocument GetHtmlDoc(Uri uri)
        {
            return GetHtmlDoc(uri, false, null);
        }

        public static HtmlDocument GetHtmlDoc(
            Uri uri,
            bool setUserAgent,
            IEnumerable<KeyValuePair<string, string>> headers
            )
        {
            HtmlDocument doc = new HtmlDocument();

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);

            if (setUserAgent)
            {
                const string agentText = "MadronaJobScraper/0.1";
                if (req.UserAgent == null)
                {
                    req.UserAgent = agentText;
                }
                else
                {
                    req.UserAgent += " " + agentText;
                }
            }

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    req.Headers.Add(header.Key, header.Value);
                }
            }

            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            {
                using (Stream htmlStream = resp.GetResponseStream())
                {
                    doc.Load(htmlStream, Encoding.UTF8);
                }
            }

            return doc;
        }

        public static string GetTextDoc(string uri)
        {
            return GetTextDoc(new Uri(uri));
        }

        public static string GetTextDoc(Uri uri)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            {
                using (StreamReader reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static string GetTextFromPdf(string uri)
        {
            return GetTextFromPdf(new Uri(uri));
        }

        public static string GetTextFromPdf(Uri uri)
        {
            byte[] pdfData;
            using (WebClient client = new WebClient())
            {
                pdfData = client.DownloadData(uri);
            }

            string jdText;
            using (PdfReader reader = new PdfReader(pdfData))
            {
                jdText = PdfTextExtractor.GetTextFromPage(reader, 1);
            }

            return jdText;
        }

        public static string GetCleanTextFromHtml(HtmlNode htmlNode)
        {
            return GetCleanTextFromHtml(htmlNode.InnerText);
        }

        public static string GetCleanTextFromHtml(string rawText)
        {
            string text = WebUtility.HtmlDecode(rawText).Trim();
            text = text.Replace("\\u000a", " ").Replace("\\u000d", " ");
            return Regex.Replace(text, "\\s+", " ");
        }
    }
}