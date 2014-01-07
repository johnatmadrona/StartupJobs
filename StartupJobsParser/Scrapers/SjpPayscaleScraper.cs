using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace StartupJobsParser
{
    public class SjpPayscaleScraper : SjpScraper
    {
        private static readonly Uri _defaultUri = new Uri("http://www.payscale.com/about/jobs");

        public override string CompanyName { get { return "Payscale"; } }
        public override Uri DefaultScrapeUri { get { return _defaultUri; } }
        public override Uri PublicUri { get { return _defaultUri; } }

        public SjpPayscaleScraper(SjpScraperParams scraperParams)
            : base(scraperParams)
        {
        }

        // WARNING: For some reason, stepping through during debugging 
        // causes HtmlNode to behave improperly. For example, Remove() 
        // doesn't remove nodes.
        private HtmlDocument GetNextTarget(HtmlNode rootNode, int targetInstance)
        {
            HtmlNode result = rootNode.CloneNode(true);
            HtmlNode next = result;
            int currentInstance = -1;

            HtmlNode remove = null;

            // Remove nodes until we find the beginning of the text
            while (currentInstance < targetInstance)
            {
                if (next.HasChildNodes)
                {
                    next = next.FirstChild;
                }
                else if (next.NextSibling != null)
                {
                    remove = next;
                    next = next.NextSibling;
                    remove.ParentNode.RemoveChild(remove, false);
                }
                else
                {
                    bool found = false;
                    while (!found && next.ParentNode != null)
                    {
                        if (next.ParentNode.NextSibling != null)
                        {
                            remove = next.ParentNode;
                            next = next.ParentNode.NextSibling;
                            remove.ParentNode.RemoveChild(remove, false);
                            found = true;
                        }
                        else
                        {
                            next = next.ParentNode;
                        }
                    }
                    if (!found)
                    {
                        // Ran out of nodes to search
                        return null;
                    }
                }

                if (next.Attributes["class"] != null &&
                    next.Attributes["class"].Value == "jobListHeader" &&
                    (next.ParentNode.OriginalName == "p" || next.ParentNode.OriginalName == "div"))
                {
                    // Found start
                    currentInstance++;
                }
            }

            // Iterate over nodes until we find the end of the text
            while (next != null &&
                !Regex.Replace(next.InnerText, @"\s+", " ").Trim().Equals("Apply for this job!", StringComparison.OrdinalIgnoreCase))
            {
                if (next.HasChildNodes)
                {
                    next = next.FirstChild;
                }
                else if (next.NextSibling != null)
                {
                    next = next.NextSibling;
                }
                else
                {
                    next = next.ParentNode;
                    while (next != null)
                    {
                        if (next.NextSibling != null)
                        {
                            next = next.NextSibling;
                            break;
                        }
                        else
                        {
                            next = next.ParentNode;
                        }
                    }
                }
            }

            // Remove the 'apply' node and all other following nodes
            if (next != null)
            {
                remove = next;
                next = GetNextSiblingOrAncestralSibling(next);
                remove.ParentNode.RemoveChild(remove, false);
                while (next != null)
                {
                    remove = next;
                    next = GetNextSiblingOrAncestralSibling(next);
                    remove.ParentNode.RemoveChild(remove, false);
                }
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(result.OuterHtml);
            return doc;
        }

        private HtmlNode GetNextSiblingOrAncestralSibling(HtmlNode node)
        {
            HtmlNode next = node;
            while (next.NextSibling == null && next.ParentNode != null)
            {
                next = next.ParentNode;
            }
            return next.NextSibling;
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument jobsDoc = SjpUtils.GetHtmlDoc(uri);

            int targetInstance = 0;
            HtmlDocument nextTarget = GetNextTarget(jobsDoc.DocumentNode, targetInstance);
            while (nextTarget != null)
            {
                yield return GetPayscaleJd(nextTarget, uri);
                targetInstance++;
                nextTarget = GetNextTarget(jobsDoc.DocumentNode, targetInstance);
            }
        }

        private JobDescription GetPayscaleJd(HtmlDocument doc, Uri jdUri)
        {
            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//span[@class='jobListHeader']");

            return new JobDescription()
            {
                SourceUri = TryCreateTrackedLink(PublicTaggedUri),
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = "Seattle, WA",
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(doc.DocumentNode),
                FullHtmlDescription = doc.DocumentNode.InnerHtml
            };
        }
    }
}