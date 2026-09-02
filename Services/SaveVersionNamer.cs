using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EasySave.Services
{
    public static class SaveVersionNamer
    {
        public static string BuildFileName(IEnumerable<string> existingFileNames, string gameName)
        {
            int nextNumber = GetNextNumber(existingFileNames);
            return $"{nextNumber:D4}_{gameName}_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
        }

        private static int GetNextNumber(IEnumerable<string> existingFileNames)
        {
            var numbers = existingFileNames
                .Select(name =>
                {
                    var match = Regex.Match(name, @"^(\d+)_");
                    return match.Success ? int.Parse(match.Groups[1].Value) : 0;
                })
                .ToList();

            return numbers.Count > 0 ? numbers.Max() + 1 : 1;
        }
    }
}
