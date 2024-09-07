using System.Diagnostics;
using Serilog;

namespace UpdateServices.Services
{
    public class ApplicationManager
    {
        public void StopRunningApplication(string exeFileName)
        {
            var processName = Path.GetFileNameWithoutExtension(exeFileName);
            var runningProcesses = Process.GetProcessesByName(processName);

            foreach (var process in runningProcesses)
            {
                try
                {
                    Log.Information($"Stopping running instance of {exeFileName} (PID: {process.Id})");
                    process.Kill();
                    process.WaitForExit(); // Optionally wait for the process to exit
                    Log.Information($"Successfully stopped {exeFileName}");
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to stop {exeFileName}: {ex.Message}");
                }
            }
        }

        public Version GetCurrentVersion(string exeFilePath)
        {
            if (File.Exists(exeFilePath))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(exeFilePath);
                Log.Information($"Successfully retrieved current version from executable: {versionInfo.ProductVersion} - {exeFilePath}");
                return new Version(versionInfo.ProductVersion!);
            }

            // Log a more explicit message if the executable is not found
            Log.Warning($"Executable file not found at path: {exeFilePath}. Default version (0.0.0.0) will be used.");
            return new Version("0.0.0.0");
        }

        public void RestartApplication(string exePath)
        {
            Log.Information($"Restarting application: {exePath}");
            Process.Start(exePath); // Launch the updated application
            Environment.Exit(0); // Terminate the current process
        }
    }
}

