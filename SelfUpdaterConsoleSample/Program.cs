using System.Diagnostics;
using System.Reflection;
using UpdateServices.Config;
using UpdateServices.Services;

var updateChecker = new UpdateChecker();

while (true)
{
    Console.WriteLine("Checking for updates...");

    // Get the current version of the running assembly
    var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
    Console.WriteLine($"Current version: {currentVersion}");
    var configFilePath = "updateservice.json";
    var config = UpdateServiceConfig.GetInstance(configFilePath);
    if (config == null)
        return;
    var latestUpdate = await updateChecker.CheckForUpdateAsync(config.UpdateCheckUrl, currentVersion!);

    if (latestUpdate != null)
    {
        Console.WriteLine($"Update found! Version {latestUpdate.Version} is available. Would you like to update now? (y/n): ");

        // Ask for user input
        var input = Console.ReadLine();

        // If user inputs 'y' (case insensitive), proceed with the update
        if (input?.Trim().ToLower() == "y")
        {
            Console.WriteLine("Launching UpdateManager.exe...");

            // Start the UpdateManager.exe from the parent directory
            Process.Start("UpdateManager.exe");

            // Exit this process to allow UpdateManager to complete the update
            Environment.Exit(0);
        }
        else
        {
            Console.WriteLine("Update cancelled by user. Checking again in 30 seconds.");
        }
    }
    else
    {
        Console.WriteLine("No updates found. Checking again in 30 seconds.");
    }
   
    await Task.Delay(TimeSpan.FromSeconds(30));  // Periodically check every 30 seconds
}


