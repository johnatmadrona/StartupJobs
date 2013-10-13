using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

namespace StartupJobsParser
{
    public class SjpStorageDisk : ISjpStorage
    {
        private string m_rootPath;

        public SjpStorageDisk(string rootDirPath)
        {
            // Use full path
            m_rootPath = Path.GetFullPath(rootDirPath);
            if (!m_rootPath.EndsWith("\\"))
            {
                m_rootPath += "\\";
            }
            if (!Directory.Exists(m_rootPath))
            {
                Directory.CreateDirectory(m_rootPath);
            }
        }

        private string PathFromKey(string key)
        {
            return m_rootPath + key;
        }

        private string KeyFromPath(string path)
        {
            return path.Substring(m_rootPath.Length);
        }

        public IEnumerable<string> List()
        {
            return List(null);
        }

        public IEnumerable<string> List(string prefix)
        {
            string dirPath = m_rootPath;
            if (prefix != null)
            {
                dirPath += prefix;
            }

            if (Directory.Exists(dirPath))
            {
                foreach (string objPath in Directory.GetFiles(dirPath))
                {
                    yield return KeyFromPath(objPath);
                }
            }
        }

        public void Add(string key, Type type, object obj)
        {
            string path = PathFromKey(key);

            string dirPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            DataContractJsonSerializer ser = new DataContractJsonSerializer(type);
            using (FileStream file = File.Create(path))
            {
                ser.WriteObject(file, obj);
            }
        }

        public bool Exists(string key)
        {
            return File.Exists(PathFromKey(key));
        }

        public object Get(string key, Type type)
        {
            string path = PathFromKey(key);
            if (!File.Exists(path))
            {
                return null;
            }

            DataContractJsonSerializer ser = new DataContractJsonSerializer(type);
            using (FileStream file = File.OpenRead(path))
            {
                return ser.ReadObject(file);
            }
        }

        public void Delete(string key)
        {
            File.Delete(PathFromKey(key));
        }
    }
}