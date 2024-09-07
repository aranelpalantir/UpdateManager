using Serilog;

namespace UpdateServices.Services
{
    public class ProductionArtifactManager
    {
        public void CopyProductionArtifacts(string appFolder, List<string> productionArtifacts)
        {
            Log.Information($"Copying Production Artifacts to Application Folder");
            var rootFolder = Directory.GetCurrentDirectory(); // Current directory is considered the root

            foreach (var item in productionArtifacts)
            {
                var fullPath = Path.IsPathRooted(item) ? item : Path.Combine(rootFolder, item);

                // Check if it's a file
                if (File.Exists(fullPath))
                {
                    // Determine if the item is a rooted (absolute) path
                    var relativePath = Path.IsPathRooted(item) ? "" : Path.GetDirectoryName(item);

                    var destinationDir = Path.Combine(appFolder, relativePath!); // Create destination folder path

                    Directory.CreateDirectory(destinationDir); // Ensure the destination directory exists

                    var destinationPath = Path.Combine(destinationDir, Path.GetFileName(fullPath)); // Final file path in the destination
                    File.Copy(fullPath, destinationPath, true);
                    Log.Information($"File copied from {fullPath} to {destinationPath}");
                }
                // Check if it's a directory
                else if (Directory.Exists(fullPath))
                {
                    // Determine if the item is a rooted (absolute) path
                    var destinationDir = Path.IsPathRooted(item)
                        ? Path.Combine(appFolder, Path.GetFileName(item))
                        : Path.Combine(appFolder, item);
                    CopyDirectory(fullPath, destinationDir);
                    Log.Information($"Directory copied from {fullPath} to {destinationDir}");
                }
                else
                {
                    Log.Warning($"File or directory not found: {fullPath}");
                }
            }
        }
   
        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir); // Ensure destination directory exists

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destinationFile = Path.Combine(destinationDir, Path.GetFileName(file));
                File.Copy(file, destinationFile, true);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                var destinationSubDir = Path.Combine(destinationDir, Path.GetFileName(directory));
                CopyDirectory(directory, destinationSubDir); // Recursive call to copy subdirectories
            }
        }
    }
}

