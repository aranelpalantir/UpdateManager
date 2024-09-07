using System.IO.Compression;
using Serilog;
using UpdateServices.Config;

namespace UpdateServices.Services
{
    public class BackupManager
    {
        private readonly UpdateServiceConfig _config;

        public BackupManager(UpdateServiceConfig config)
        {
            _config = config;
        }

        public void BackupCurrentVersion(string appFolder, string backupFolder, string version)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var backupZipPath = Path.Combine(backupFolder, $"{version}_{timestamp}.zip");

            var filesToBackup = GetFilesToBackup(appFolder, _config.ExcludeFromBackup!);
            ZipFiles(filesToBackup, backupZipPath);
        }

        private List<string> GetFilesToBackup(string sourceDir, List<string> excludePaths)
        {
            var allFiles = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories).ToList();

            // Normalize excluded paths to ensure they're absolute paths
            var excludedFullPaths = excludePaths
                .Select(path => Path.Combine(sourceDir, path))
                .Select(fullPath => Path.GetFullPath(fullPath)) // Normalize to absolute paths
                .ToList();

            return allFiles
                .Where(file =>
                {
                    var fullFilePath = Path.GetFullPath(file);

                    // Exclude the file if it matches exactly any excluded file path or is part of an excluded directory
                    return !excludedFullPaths.Any(exclude =>
                        fullFilePath.Equals(exclude, StringComparison.OrdinalIgnoreCase) || // Exact file match
                        fullFilePath.StartsWith(exclude + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)); // Part of excluded directory
                })
                .ToList();
        }

        private void ZipFiles(List<string> files, string zipPath)
        {
            using (var zipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (var file in files)
                {
                    var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), file);
                    zipArchive.CreateEntryFromFile(file, relativePath);
                }
            }

            Log.Information($"Backup created at {zipPath}");
        }
    }
}
