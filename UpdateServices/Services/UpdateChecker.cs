using System.Text.Json;
using Serilog;
using UpdateServices.Models;

namespace UpdateServices.Services
{
    public class UpdateChecker
    {
        public async Task<UpdateInfo?> CheckForUpdateAsync(string updateCheckUrl, Version currentVersion)
        {
            if (Uri.IsWellFormedUriString(updateCheckUrl, UriKind.Absolute))
            {
                return await CheckForUpdateFromUrlAsync(updateCheckUrl, currentVersion);
            }
            else if (Directory.Exists(updateCheckUrl) || File.Exists(updateCheckUrl))
            {
                return CheckForUpdateFromLocalPath(updateCheckUrl, currentVersion);
            }
            else
            {
                Log.Error($"Invalid update path or URL: {updateCheckUrl}");
                return null;
            }
        }

        private async Task<UpdateInfo?> CheckForUpdateFromUrlAsync(string updateCheckUrl, Version currentVersion)
        {
            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetStringAsync(updateCheckUrl);
                var updateInfo = JsonSerializer.Deserialize<UpdateInfo>(response);

                if (Version.TryParse(updateInfo?.Version, out var latestVersion) && latestVersion > currentVersion)
                {
                    return updateInfo; 
                }

                Log.Information("No update available from URL.");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to check for update from URL: {ex.Message}");
                return null;
            }
        }

        private UpdateInfo? CheckForUpdateFromLocalPath(string updateCheckPath, Version currentVersion)
        {
            try
            {
                if (!File.Exists(updateCheckPath))
                {
                    Log.Warning($"Update info file not found: {updateCheckPath}");
                    return null;
                }

                var updateInfoJson = File.ReadAllText(updateCheckPath);
                var updateInfo = JsonSerializer.Deserialize<UpdateInfo>(updateInfoJson);

                if (Version.TryParse(updateInfo?.Version, out var latestVersion) && latestVersion > currentVersion)
                {
                    return updateInfo;
                }

                Log.Information("No update available from local/network path.");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to check for update from local path: {ex.Message}");
                return null;
            }
        }

        public async Task<string?> DownloadOrCopyUpdateAsync(UpdateInfo updateInfo, string updatesFolder)
        {
            if (Uri.IsWellFormedUriString(updateInfo.DownloadUrl, UriKind.Absolute))
            {
                return await DownloadUpdateAsync(updateInfo.DownloadUrl, updatesFolder);
            }
            else if (File.Exists(updateInfo.DownloadUrl))
            {
                return CopyUpdate(updateInfo.DownloadUrl, updatesFolder);
            }
            else
            {
                Log.Error($"Invalid update download path or URL: {updateInfo.DownloadUrl}");
                return null;
            }
        }

        private async Task<string?> DownloadUpdateAsync(string downloadUrl, string updatesFolder)
        {
            try
            {
                var updateFileName = Path.GetFileName(downloadUrl);
                var latestUpdatePath = Path.Combine(updatesFolder, updateFileName);

                if (File.Exists(latestUpdatePath))
                {
                    latestUpdatePath = Path.Combine(updatesFolder, $"{DateTime.Now:yyyyMMddHHmmss}_{updateFileName}");
                }

                using var httpClient = new HttpClient();
                var updateFileBytes = await httpClient.GetByteArrayAsync(downloadUrl);
                await File.WriteAllBytesAsync(latestUpdatePath, updateFileBytes);

                Log.Information($"New update downloaded to: {latestUpdatePath}");
                return latestUpdatePath;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to download update: {ex.Message}");
                return null;
            }
        }

        private string? CopyUpdate(string updateFilePath, string updatesFolder)
        {
            try
            {
                var updateFileName = Path.GetFileName(updateFilePath);
                var latestUpdatePath = Path.Combine(updatesFolder, updateFileName);

                if (File.Exists(latestUpdatePath))
                {
                    latestUpdatePath = Path.Combine(updatesFolder, $"{DateTime.Now:yyyyMMddHHmmss}_{updateFileName}");
                }

                File.Copy(updateFilePath, latestUpdatePath, true);

                Log.Information($"New update copied to: {latestUpdatePath}");
                return latestUpdatePath;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to copy update: {ex.Message}");
                return null;
            }
        }
    }
}

