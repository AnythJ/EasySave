using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace EasySave.Services
{
    public static class SaveArchiver
    {
        public static string CreateZip(List<string> filePaths, string zipPath)
        {
            using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
            foreach (var filePath in filePaths)
                archive.CreateEntryFromFile(filePath, Path.GetFileName(filePath));

            return zipPath;
        }

        public static void ExtractZip(string zipPath, string destinationFolder)
        {
            Directory.CreateDirectory(destinationFolder);

            using var archive = ZipFile.OpenRead(zipPath);
            foreach (var entry in archive.Entries)
            {
                string destPath = Path.Combine(destinationFolder, entry.Name);
                entry.ExtractToFile(destPath, overwrite: true);
            }
        }
    }
}
