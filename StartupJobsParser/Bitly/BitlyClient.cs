using System;
using System.Collections;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Web;

namespace StartupJobsParser
{
    public class BitlyClient : ISjpLinkTracker
    {
        private HttpClient _client = new HttpClient();
        private string _accessToken;

        private Hashtable _cache = new Hashtable(1024);

        public BitlyClient(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("Access token must be provided", "accessToken");
            }

            _accessToken = accessToken;
        }

        private BitlyClient()
        {
        }

        #region ISjpLinkTracker
        public string CreateTrackedLink(string url)
        {
            return Shorten(url);
        }

        public Task<string> CreateTrackedLinkAsync(string url)
        {
            return ShortenAsync(url);
        }
        #endregion

        public string Shorten(string urlToShorten)
        {
            Task<string> async = ShortenAsync(urlToShorten);
            async.Wait();
            return async.Result;
        }

        // TODO: Exception handling
        public async Task<string> ShortenAsync(string urlToShorten)
        {
            if (_cache.ContainsKey(urlToShorten))
            {
                return (string)_cache[urlToShorten];
            }

            string bitlyUrl = CreateBitlyUrl(urlToShorten);
            HttpResponseMessage msg = await _client.GetAsync(bitlyUrl);

            string shortened;
            using (Stream content = await msg.Content.ReadAsStreamAsync())
            {
                DataContractJsonSerializer ser =
                    new DataContractJsonSerializer(typeof(BitlyShortenResponse));
                BitlyShortenResponse resp = ser.ReadObject(content) as BitlyShortenResponse;
                shortened = resp.data.url;
            }

            _cache.Add(urlToShorten, shortened);
            return shortened;
        }

        private string CreateBitlyUrl(string urlToShorten)
        {
            return "https://api-ssl.bitly.com/v3/shorten?" + 
                "access_token=" + _accessToken + 
                "&longUrl=" + HttpUtility.UrlEncode(urlToShorten);
        }
    }
}