using Serilog;
using System.Text.Json;
using UpdateServices.Config;
using UpdateServices.Services;


var tempLogPath = Path.Combine(Directory.GetCurrentDirectory(), "temp.log");

// Temporary logging configuration
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(tempLogPath, rollingInterval: RollingInterval.Day)
    .CreateLogger();
try
{
    var configFilePath = "updateservice.json";
    var config = UpdateServiceConfig.GetInstance(configFilePath);
    if (config == null)
        return;

    // Serilog configuration for console and file logging with rotation

    var logFolderPath = Path.IsPathRooted(config.LogsFolder) ? config.LogsFolder : Path.Combine(Directory.GetCurrentDirectory(), config.LogsFolder!);
    Directory.CreateDirectory(logFolderPath);

    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console() // Logs to console
        .WriteTo.File(
            Path.Combine(logFolderPath, "application.log"), // Log file path
            rollingInterval: RollingInterval.Day, // Rotate log file daily
            fileSizeLimitBytes: 1024 * 1024, // Set file size limit to 1 MB
            rollOnFileSizeLimit: true, // Rotate file when size limit is reached
            retainedFileCountLimit: 7 // Keep 7 days of log files
        )
        .CreateLogger();

    Log.Information("Starting the application...");

    // Instantiate individual services
    var backupManager = new BackupManager(config);
    var updateChecker = new UpdateChecker();
    var applicationManager = new ApplicationManager();
    var artifactManager = new ProductionArtifactManager();

    // Inject dependencies into UpdateService
    var updateManager = new UpdateService(
        config,
        backupManager,
        updateChecker,
        applicationManager,
        artifactManager
    );

    // Run the update process
    await updateManager.RunUpdateProcessAsync();
}
catch (JsonException jsonEx)
{
    Log.Error(jsonEx, "Error parsing the configuration file.");
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unexpected error occurred.");
}
finally
{
    Log.CloseAndFlush(); // Ensures all logs are flushed and written before application exit
}