using System;
using System.IO;

namespace Yandex.Utils
{
    public static class PathResolver
    {
        private const string LogMapPath = @"C:\$EDWD_logs\LogsMap.txt";

        public static string GetPath(string pathId)
        {
            if (pathId == null) throw new ArgumentNullException("pathId");

            var reader = new StreamReader(LogMapPath);
            string res = null;
            while (reader.Peek() != -1)
            {
                var prop = reader.ReadLine();
                prop = prop.Substring(0, prop.IndexOf('/'));
                var arr = prop.Split('=');
                if (!pathId.Equals(arr[0].Trim())) continue;
                res = arr[1].Trim();
                break;
            }
            if (res == null) throw new Exception("Property " + pathId + " not found in " + LogMapPath);
            return res;
        }
    }
}