using System.Runtime.Serialization;

namespace StartupJobsParser
{
    [DataContract]
    public class BitlyShortenResponse
    {
        [DataMember]
        public BitlyShortenResponseData data { get; set; }

        [DataMember]
        public int status_code { get; set; }

        [DataMember]
        public string status_txt { get; set; }
    }
}