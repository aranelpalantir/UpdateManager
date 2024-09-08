using System.IO.Compression;
using Serilog;
using UpdateServices.Config;

namespace UpdateServices.Services
{
    public class UpdateService
    {
        private readonly UpdateServiceConfig _config;
        private readonly BackupManager _backupManager;
        private readonly UpdateChecker _updateChecker;
        private readonly ApplicationManager _applicationManager;
        private readonly ProductionArtifactManager _productionArtifactManager;

        public UpdateService(UpdateServiceConfig config, BackupManager backupManager, UpdateChecker updateChecker, ApplicationManager applicationManager, ProductionArtifactManager productionArtifactManager)
        {
            _config = config;
            _backupManager = backupManager;
            _updateChecker = updateChecker;
            _applicationManager = applicationManager;
            _productionArtifactManager = productionArtifactManager;
        }

        public async Task RunUpdateProcessAsync()
        {
            Log.Information("Update process started.");

            var currentVersion = _applicationManager.GetCurrentVersion(Path.Combine(_config.ApplicationFolder!, _config.ExeFileName));

            // Check for updates
            var latestUpdate = await _updateChecker.CheckForUpdateAsync(_config.UpdateCheckUrl, currentVersion);

            if (latestUpdate != null)
            {
                if (_config.AutoStopApplication!.Value) 
                    if (string.IsNullOrWhiteSpace(_config.ApplicationPoolName))
                        _applicationManager.StopRunningApplication(_config.ExeFileName);
                    else
                        await _applicationManager.StopRunningApplicationPoolAsync(_config.ApplicationPoolName);

                // Ensure that required directories exist
                EnsureDirectoriesExist(new[] { _config.UpdatesFolder!, _config.BackupsFolder! });

                var latestUpdatePath = await _updateChecker.DownloadOrCopyUpdateAsync(latestUpdate, _config.UpdatesFolder!);
                HandleInitialSetupOrBackup(latestUpdatePath, currentVersion);

                if (_config.AutoRestartApplication!.Value)
                    if (string.IsNullOrWhiteSpace(_config.ApplicationPoolName))
                        _applicationManager.RestartApplication(Path.Combine(_config.ApplicationFolder!,
                            _config.ExeFileName));
                    else
                        await _applicationManager.StartApplicationPoolAsync(_config.ApplicationPoolName);
            }

            Log.Information("Update process completed successfully.");
        }

        private void HandleInitialSetupOrBackup(string? latestUpdatePath, Version currentVersion)
        {
            if (!Directory.GetFiles(_config.ApplicationFolder!).Any())
            {
                Log.Information("Performing initial setup.");
            }
            else
            {
                Log.Information($"Backing up current version: {currentVersion}");
                _backupManager.BackupCurrentVersion(_config.ApplicationFolder!, _config.BackupsFolder!, currentVersion.ToString());
            }

            if (latestUpdatePath != null)
            {
                Log.Information("Applying the latest update.");
                ApplyUpdate(latestUpdatePath, _config.ApplicationFolder!);
            }

            // Copy production files or directories after applying the update
            _productionArtifactManager.CopyProductionArtifacts(_config.ApplicationFolder!, _config.ProductionArtifacts!);
        }

        private void ApplyUpdate(string updatePath, string appFolder)
        {
            if (Directory.Exists(appFolder) && Directory.GetFiles(appFolder).Any())
            {
                // Existing application files are present
                Log.Information("Preparing for update by deleting old application files (except excluded ones).");
                DeleteApplicationFilesExceptExcluded(appFolder, _config.ExcludeFromDelete!);
            }
            else
            {
                // No existing application files, likely first setup
                Log.Information("No existing application files found. Proceeding with first-time installation.");
            }

            // Recreate the application folder
            Directory.CreateDirectory(appFolder);

            // Extract the update files
            ExtractZipAndMoveFiles(updatePath, appFolder);
        }
        private void ExtractZipAndMoveFiles(string updatePath, string appFolder)
        {
            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);

            try
            {
                ZipFile.ExtractToDirectory(updatePath, tempFolder);
                Log.Information($"Extracted update to temporary folder: {tempFolder}");
                MoveExtractedFiles(tempFolder, appFolder);
            }
            finally
            {
                Directory.Delete(tempFolder, true); // Cleanup temporary folder
                Log.Information("Temporary folder cleaned up.");
            }
        }

        private void MoveExtractedFiles(string tempFolder, string appFolder)
        {
            var extractedDirectories = Directory.GetDirectories(tempFolder);
            var extractedFiles = Directory.GetFiles(tempFolder);

            // Handle cases where all extracted files are in a subfolder
            if (extractedDirectories.Length == 1 && extractedFiles.Length == 0)
            {
                Log.Information("Moving files from inner folder.");
                MoveFilesFromInnerFolder(extractedDirectories[0], appFolder);
            }
            else
            {
                Log.Information("Moving files directly from extracted folder.");
                MoveFilesAndDirectories(tempFolder, appFolder);
            }
        }

        // Move files from an inner folder to the target application folder
        private void MoveFilesFromInnerFolder(string innerFolder, string appFolder)
        {
            foreach (var file in Directory.GetFiles(innerFolder))
            {
                File.Move(file, Path.Combine(appFolder, Path.GetFileName(file)));
            }

            foreach (var directory in Directory.GetDirectories(innerFolder))
            {
                Directory.Move(directory, Path.Combine(appFolder, Path.GetFileName(directory)));
            }

            Log.Information($"Files moved from inner folder: {innerFolder}");
        }

        // Move all files and directories from the temporary folder to the target application folder
        private void MoveFilesAndDirectories(string sourceFolder, string targetFolder)
        {
            foreach (var file in Directory.GetFiles(sourceFolder))
            {
                var destinationFileName = Path.Combine(targetFolder, Path.GetFileName(file));
                if (!File.Exists(destinationFileName))
                    File.Copy(file, destinationFileName, false);
                else
                    Log.Warning($"File was not moved because it already exists at the destination: {destinationFileName}");
            }

            foreach (var directory in Directory.GetDirectories(sourceFolder))
            {
                var destinationDirectoryName = Path.Combine(targetFolder, Path.GetFileName(directory));
                if (!Directory.Exists(destinationDirectoryName))
                {
                    CopyDirectory(directory, destinationDirectoryName, true);
                }
                else
                    Log.Warning($"Directory was not moved because it already exists at the destination: {destinationDirectoryName}");

            }

            Log.Information($"Files moved from: {sourceFolder}");
        }

        private void DeleteApplicationFilesExceptExcluded(string appFolder, List<string> excludePaths)
        {
            var allFiles = Directory.GetFiles(appFolder, "*", SearchOption.AllDirectories).ToList();
            var allDirs = Directory.GetDirectories(appFolder, "*", SearchOption.AllDirectories).OrderByDescending(d => d.Length).ToList(); // Order directories by path length

            var excludedFullPaths = excludePaths.Select(path => Path.Combine(appFolder, path)).ToList();

            // Delete files that are not in the excluded paths
            foreach (var file in allFiles)
            {
                if (!excludedFullPaths.Any(exclude => file.StartsWith(exclude, StringComparison.OrdinalIgnoreCase)))
                {
                    File.Delete(file);
                    Log.Information($"Deleted file: {file}");
                }
            }

            // Delete directories that are not in the excluded paths
            foreach (var dir in allDirs)
            {
                if (!excludedFullPaths.Any(exclude => dir.StartsWith(exclude, StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        Directory.Delete(dir, true);
                        Log.Information($"Deleted directory: {dir}");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Failed to delete directory: {dir}. Exception: {ex.Message}");
                    }
                }
            }
        }

        private void EnsureDirectoriesExist(string[] directories)
        {
            foreach (var dir in directories)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    Log.Information($"Directory created: {dir}");
                }
            }
        }
        private void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Create the destination directory if it doesn't exist
            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            // Get all files in the source directory and copy them to the destination
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFileName = Path.Combine(destinationDir, Path.GetFileName(file));
                File.Copy(file, destFileName, true);  // Overwrite if file already exists
            }

            // If recursive, copy all subdirectories recursively
            if (recursive)
            {
                foreach (var dir in Directory.GetDirectories(sourceDir))
                {
                    var destDirName = Path.Combine(destinationDir, Path.GetFileName(dir));
                    CopyDirectory(dir, destDirName, recursive);
                }
            }
        }
    }
}
