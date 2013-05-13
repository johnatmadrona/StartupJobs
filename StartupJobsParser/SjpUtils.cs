using HtmlAgilityPack;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.IO;
using System.Net;

namespace StartupJobsParser
{
    public static class SjpUtils
    {
        public static HtmlDocument GetHtmlDoc(string link)
        {
            HtmlDocument doc = new HtmlDocument();

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(link);
            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            {
                using (Stream htmlStream = resp.GetResponseStream())
                {
                    doc.Load(htmlStream);
                }
            }

            return doc;
        }

        public static string GetTextFromPdf(string link)
        {
            byte[] pdfData;
            using (WebClient client = new WebClient())
            {
                pdfData = client.DownloadData(link);
            }

            string jdText;
            using (PdfReader reader = new PdfReader(pdfData))
            {
                jdText = PdfTextExtractor.GetTextFromPage(reader, 1);
            }

            return jdText;
        }
    }
}