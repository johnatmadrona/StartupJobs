using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.IO.Compression;

namespace StartupJobsParser
{
    public class SjpStorageS3 : ISjpStorage, IDisposable
    {
        AmazonS3Client m_client;
        string m_bucketName;

        public SjpStorageS3(
            RegionEndpoint region,
            string bucketName
            )
        {
            Init(new AmazonS3Client(region), bucketName);
        }

        public SjpStorageS3(
            string awsAccessKey,
            string awsSecretAccesskey,
            RegionEndpoint region,
            string bucketName
            )
        {
            Init(new AmazonS3Client(awsAccessKey, awsSecretAccesskey, region), bucketName);
        }

        private void Init(AmazonS3Client client, string bucketName)
        {
            m_client = client;
            m_bucketName = bucketName;

            DeleteBucketRequest req1 = new DeleteBucketRequest()
            {
                BucketName = m_bucketName
            };

            if (!AmazonS3Util.DoesS3BucketExist(m_client, m_bucketName))
            {
                PutBucketRequest req = new PutBucketRequest()
                {
                    BucketName = m_bucketName,
                    UseClientRegion = true,
                    CannedACL = S3CannedACL.PublicRead
                };
                PutBucketResponse res = m_client.PutBucket(req);
            }
        }

        #region IDisposable
        ~SjpStorageS3()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_client.Dispose();
            }
        }
        #endregion

        #region ISjpStorage
        public IEnumerable<string> List()
        {
            return List(null);
        }

        public IEnumerable<string> List(string prefix)
        {
            ListObjectsRequest req = new ListObjectsRequest()
            {
                BucketName = m_bucketName
            };
            if (prefix != null)
            {
                req.Prefix = prefix;
            }

            bool more = true;
            while (more)
            {
                ListObjectsResponse res = m_client.ListObjects(req);
                foreach (S3Object obj in res.S3Objects)
                {
                    yield return obj.Key;
                }
                more = res.IsTruncated;
            }
        }

        public void Add(string key, Type type, object obj)
        {
            PutObjectRequest req = new PutObjectRequest()
            {
                BucketName = m_bucketName,
                Key = key,
                CannedACL = S3CannedACL.PublicRead
            };

            const int TwoWeeks = 60 * 60 * 24 * 7;
            req.Headers.CacheControl = string.Format("max-age={0}", TwoWeeks);

            DataContractJsonSerializer ser = new DataContractJsonSerializer(type);
            using (MemoryStream data = new MemoryStream())
            {
                // TODO: Compress using GZipOutputStream
                ser.WriteObject(data, obj);
                data.Position = 0;
                req.InputStream = data;
                m_client.PutObject(req);
            }
        }

        public bool Exists(string key)
        {
            ListObjectsRequest req = new ListObjectsRequest()
            {
                BucketName = m_bucketName,
                Prefix = key
            };

            ListObjectsResponse res = m_client.ListObjects(req);
            foreach (S3Object obj in res.S3Objects)
            {
                if (obj.Key == key)
                {
                    return true;
                }
            }

            return false;
        }

        public object Get(string key, Type type)
        {
            GetObjectRequest req = new GetObjectRequest()
            {
                BucketName = m_bucketName,
                Key = key
            };

            // TODO: Handle non-existant object
            DataContractJsonSerializer ser = new DataContractJsonSerializer(type);
            using (GetObjectResponse res = m_client.GetObject(req))
            {
                return ser.ReadObject(res.ResponseStream);
            }
        }

        public void Delete(string key)
        {
            DeleteObjectRequest req = new DeleteObjectRequest()
            {
                BucketName = m_bucketName,
                Key = key
            };
            m_client.DeleteObject(req);
        }
        #endregion
    }
}