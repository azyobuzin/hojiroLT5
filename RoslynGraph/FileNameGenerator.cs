using System;
using System.IO;
using System.Text;

namespace RoslynGraph
{
    internal static class FileNameGenerator
    {
        private static char[] s_invalidFileNameChars;

        static FileNameGenerator()
        {
            var invalidFileNameChars = Path.GetInvalidFileNameChars();
            s_invalidFileNameChars = new char[invalidFileNameChars.Length + 1];
            invalidFileNameChars.CopyTo(s_invalidFileNameChars, 0);
            s_invalidFileNameChars[invalidFileNameChars.Length] = '.';
            Array.Sort(s_invalidFileNameChars);
        }

        public static string CreateFilePath(string dir, string name, string extension)
        {
            var sb = new StringBuilder(name.Length + (extension?.Length ?? 0) + 1);

            foreach (var c in name)
            {
                sb.Append(
                    Array.BinarySearch(s_invalidFileNameChars, c) < 0
                        ? c : '_'
                );
            }

            if (!string.IsNullOrEmpty(extension))
            {
                if (extension[0] != '.') sb.Append('.');
                sb.Append(extension);
            }

            var result = sb.ToString();

            return string.IsNullOrEmpty(dir)
                ? result
                : Path.Combine(dir, result);
        }
    }
}
