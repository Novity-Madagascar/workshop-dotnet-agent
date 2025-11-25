using DocumentAssistantAgent.Core.Models;
using System.IO.Compression;

namespace DocumentAssistantAgent.Infrastructure.Utils
{
    public static class ZipUtils
    {
        /// <summary>
        /// Creates a zip file from a list of DocumentInfo objects.
        /// </summary>
        /// <param name="documents"></param>
        /// <param name="zipPath"></param>
        /// <returns></returns>
        public static async Task CreateZipFileAsync(List<DocumentInfo> documents, string zipPath)
        {
            // Delete existing zip file if it exists
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            using MemoryStream stream = new();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                foreach (var doc in documents)
                {
                    if (File.Exists(doc.FullPath))
                    {
                        // Use a safe filename to avoid duplicates
                        //var entryName = GetSafeEntryName(doc.FileName, archive);
                        archive.CreateEntryFromFile(doc.FullPath, doc.FileName);
                    }
                }
            }

            await File.WriteAllBytesAsync(zipPath, stream.ToArray());

            await Task.CompletedTask;
        }

        /// <summary>
        /// Generates a safe entry name for the zip archive to avoid duplicates.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="archive"></param>
        /// <returns></returns>
        private static string GetSafeEntryName(string fileName, ZipArchive archive)
        {
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var counter = 1;
            var entryName = fileName;

            while (archive.Entries.Any(e => e.Name == entryName))
            {
                entryName = $"{baseName}_{counter}{extension}";
                counter++;
            }

            return entryName;
        }
    }
}
