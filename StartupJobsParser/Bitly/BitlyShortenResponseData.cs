using System.Runtime.Serialization;

namespace StartupJobsParser
{
    [DataContract]
    public class BitlyShortenResponseData
    {
        [DataMember]
        public string global_hash { get; set; }

        [DataMember]
        public string hash { get; set; }

        [DataMember]
        public string long_url { get; set; }

        [DataMember]
        public string new_hash { get; set; }

        [DataMember]
        public string url { get; set; }
    }
}