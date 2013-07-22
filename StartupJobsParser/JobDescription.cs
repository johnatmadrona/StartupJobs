﻿using System.Runtime.Serialization;

namespace StartupJobsParser
{
    [DataContract]
    public class JobDescription
    {
        [DataMember]
        public string SourceUri { get; set; }

        [DataMember]
        public string Company { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Location { get; set; }

        [DataMember]
        public string FullTextDescription { get; set; }

        [DataMember]
        public string FullHtmlDescription { get; set; }

        [DataMember]
        public string Responsibilities { get; set; }

        [DataMember]
        public string Requirements { get; set; }
    }
}