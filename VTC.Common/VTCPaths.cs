using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VTC.Common
{
    public static class VTCPaths
    {

        public static string FolderPath(string filename, DateTime time, UserConfig userConfig)
        {
            var dt = time;
            var dateTime = "D" + dt.Day + "M" + dt.Month + "Y" + dt.Year + " H" + dt.Hour + "M" + dt.Minute + "S" + dt.Second;
            var folderName = "VTC Movement Counts " + SanitizeFilename(filename) + " " + dateTime;

            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (!string.IsNullOrEmpty(userConfig.OutputPath))
            {
                basePath = userConfig.OutputPath;
            }

            var folderPath = Path.Combine(basePath,
                folderName);
            return folderPath;
        }

        //Taken from daniweb
        //https://www.daniweb.com/programming/software-development/threads/217968/generate-safe-filenames-with-c
        public static string SanitizeFilename(string filename)
        {
            // first trim the raw string
            var safe = filename.Trim();
            // replace spaces with hyphens
            safe = safe.Replace(" ", "-").ToLower();
            // replace any 'double spaces' with singles
            if (safe.IndexOf("--", StringComparison.Ordinal) > -1)
                while (safe.IndexOf("--", StringComparison.Ordinal) > -1)
                    safe = safe.Replace("--", "-");
            // trim out illegal characters
            safe = Regex.Replace(safe, "[^a-z0-9\\-]", "");
            // trim the length
            if (safe.Length > 50)
                safe = safe.Substring(0, 49);
            // clean the beginning and end of the filename
            char[] replace = { '-', '.' };
            safe = safe.TrimStart(replace);
            safe = safe.TrimEnd(replace);
            return safe;
        }
    }
}
