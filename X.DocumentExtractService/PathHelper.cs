using System;
using System.IO;

namespace X.DocumentExtractService
{
    public static class PathHelper
    {
        public static string GetRootedPath(string path)
        {
            string text = path;
            if (!Path.IsPathRooted(text))
            {
                text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, text ?? string.Empty);
            }
            string directoryName = Path.GetDirectoryName(text);
            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            return text;
        }
    }
}