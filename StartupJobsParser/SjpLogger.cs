using System;
using System.IO;

namespace StartupJobsParser
{
    public static class SjpLogger
    {
        public static void Log(string format, params object[] args)
        {
            string text = string.Format(format, args);
            string output = string.Format(
                "{0:yyyy-MM-dd HH:mm:ss.fff}: {1}\n",
                DateTime.UtcNow,
                text
                );
            Console.Write(output);
            File.AppendAllText("log.txt", output);
        }
    }
}