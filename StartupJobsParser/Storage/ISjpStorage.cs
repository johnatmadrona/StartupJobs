using System;
using System.Collections.Generic;

public interface ISjpStorage
{
    IEnumerable<string> List();
    void Add(string id, Type type, object obj);
    bool Exists(string id);
    object Get(string id, Type type);
    void Delete(string id);
}