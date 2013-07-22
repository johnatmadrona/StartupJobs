using System;
using System.Collections.Generic;

public interface ISjpStorage
{
    IEnumerable<string> List();
    IEnumerable<string> List(string prefix);

    void Add(string key, Type type, object obj);

    bool Exists(string key);

    object Get(string key, Type type);

    void Delete(string key);
}