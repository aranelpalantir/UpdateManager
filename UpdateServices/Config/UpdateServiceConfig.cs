using Serilog;
using System.Text.Json;

namespace UpdateServices.Config
{
    public class UpdateServiceConfig
    {
        public required string ExeFileName { get; set; }
        public required string UpdateCheckUrl { get; set; }
        public string? LogsFolder { get; set; }
        public string? UpdatesFolder { get; set; }
        public string? BackupsFolder { get; set; }
        public string? ApplicationFolder { get; set; }
        public List<string>? ProductionArtifacts { get; set; }
        public List<string>? ExcludeFromBackup { get; set; }
        public List<string>? ExcludeFromDelete { get; set; }
        public bool? AutoStopApplication { get; set; }
        public bool? AutoRestartApplication { get; set; }
        public string? ApplicationPoolName { get; set; }

        public static UpdateServiceConfig? GetInstance(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                Log.Error($"Configuration file not found at path: {configFilePath}");
                return null;
            }

            var configJson = File.ReadAllText(configFilePath);
            var config = JsonSerializer.Deserialize<UpdateServiceConfig>(configJson);

            if (config == null)
            {
                Log.Error("Failed to deserialize configuration.");
                return null;
            }
            config.ApplyDefaults();
            return config;
        }

        private void ApplyDefaults()
        {
            if (string.IsNullOrWhiteSpace(LogsFolder))
            {
                LogsFolder = "Logs";
            }

            if (string.IsNullOrWhiteSpace(UpdatesFolder))
            {
                UpdatesFolder = "Updates";
            }

            if (string.IsNullOrWhiteSpace(BackupsFolder))
            {
                BackupsFolder = "Backups";
            }

            if (string.IsNullOrWhiteSpace(ApplicationFolder))
            {
                ApplicationFolder = AppContext.BaseDirectory;
            }

            if (ProductionArtifacts == null || !ProductionArtifacts.Any())
            {
                ProductionArtifacts = new List<string>();
            }

            if (ExcludeFromBackup == null || !ExcludeFromBackup.Any())
            {
                ExcludeFromBackup = new List<string>();
            }

            ExcludeFromBackup.Add(UpdatesFolder);
            ExcludeFromBackup.Add(BackupsFolder);
            ExcludeFromBackup.Add("Logs");

            if (ExcludeFromDelete == null || !ExcludeFromDelete.Any())
            {
                ExcludeFromDelete = new List<string>();
            }

            ExcludeFromDelete.Add(UpdatesFolder);
            ExcludeFromDelete.Add(BackupsFolder);
           
            ExcludeFromDelete.AddRange(new[]
            {
                "Logs",
                "Serilog.dll",
                "Serilog.Sinks.Console.dll",
                "Serilog.Sinks.File.dll",
                "UpdateManager.deps.json",
                "UpdateManager.dll",
                "UpdateManager.exe",
                "UpdateManager.runtimeconfig.json",
                "UpdateServices.dll",
                "updateservice.json"
            });

            AutoStopApplication ??= true;
            AutoRestartApplication ??= true;
        }

    }
}
