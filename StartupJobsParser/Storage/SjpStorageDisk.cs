using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

public class SjpStorageDisk : ISjpStorage
{
    private string m_dirPath;

    public SjpStorageDisk(string dirPath)
    {
        // Use full path
        m_dirPath = Path.GetFullPath(dirPath);
        if (!Directory.Exists(m_dirPath))
        {
            Directory.CreateDirectory(m_dirPath);
        }
    }

    private string PathFromId(string id)
    {
        return m_dirPath + id;
    }

    private string IdFromPath(string path)
    {
        return path.Substring(m_dirPath.Length);
    }

    public IEnumerable<string> List()
    {
        foreach (string path in Directory.GetFiles(m_dirPath))
        {
            yield return IdFromPath(path);
        }
    }

    public void Add(string id, Type type, object obj)
    {
        string path = PathFromId(id);
        DataContractJsonSerializer ser = new DataContractJsonSerializer(type);
        using (FileStream file = File.Create(path))
        {
            ser.WriteObject(file, obj);
        }
    }

    public bool Exists(string id)
    {
        return File.Exists(PathFromId(id));
    }

    public object Get(string id, Type type)
    {
        string path = PathFromId(id);
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

    public void Delete(string id)
    {
        File.Delete(PathFromId(id));
    }
}