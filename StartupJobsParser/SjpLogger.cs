using System;
using System.IO;

namespace StartupJobsParser
{
    public static class SjpLogger
    {
        private static object s_lock = new object();

        public static void Log(string format, params object[] args)
        {
            string text = string.Format(format, args);
            string output = string.Format(
                "{0:yyyy-MM-dd HH:mm:ss.fff}: {1}\n",
                DateTime.UtcNow,
                text
                );
            Console.Write(output);
            lock (s_lock)
            {
                File.AppendAllText("log.txt", output);
            }
        }
    }
}