﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace StartupJobsParser
{
    public class SjpIntrepidLearningScraper : SjpJobviteScraperBase
    {
        public override string CompanyName { get { return "Intrepid Learning"; } }
        protected override string JobviteCompanyId { get { return "q089VfwW"; } }

        public SjpIntrepidLearningScraper(string storageDirPath, ISjpIndex index)
            : base(storageDirPath, index)
        {
        }

        protected override IEnumerable<JobDescription> GetJds(Uri uri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(uri);
            foreach (HtmlNode jdUriNode in doc.DocumentNode.SelectNodes("//a[@class='jvjoblink']"))
            {
                // Get the jobvite job ID from the link
                string jobRoute = jdUriNode.Attributes["onclick"].Value;
                int idEnd = jobRoute.LastIndexOf('\'');
                int idStart = jobRoute.LastIndexOf('\'', idEnd - 1) + 1;
                string jobId = jobRoute.Substring(idStart, idEnd - idStart) + ",Job";
                yield return GetIntrepidLearningJd(GetItemUri(WebUtility.UrlEncode(jobId)));
            }
        }

        private JobDescription GetIntrepidLearningJd(Uri jdUri)
        {
            HtmlDocument doc = SjpUtils.GetHtmlDoc(jdUri);

            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//div[@class='jvheader']");

            HtmlNode locationNode = titleNode.NextSibling;
            string location = null;
            while (location == null)
            {
                if (locationNode.InnerText.Contains("|"))
                {
                    location = locationNode.InnerText.Split('|')[1];
                }
                else
                {
                    locationNode = locationNode.NextSibling;
                }
            }

            HtmlNode descriptionNode = titleNode.ParentNode;
            foreach (HtmlNode remove in descriptionNode.SelectNodes("div"))
            {
                remove.ParentNode.RemoveChild(remove, false);
            }
            foreach (HtmlNode remove in descriptionNode.SelectNodes("script"))
            {
                remove.ParentNode.RemoveChild(remove, false);
            }

            return new JobDescription()
            {
                SourceUri = jdUri.AbsoluteUri,
                Company = CompanyName,
                Title = SjpUtils.GetCleanTextFromHtml(titleNode),
                Location = WebUtility.HtmlDecode(location).Trim(),
                FullTextDescription = SjpUtils.GetCleanTextFromHtml(descriptionNode),
                FullHtmlDescription = descriptionNode.InnerHtml
            };
        }
    }
}