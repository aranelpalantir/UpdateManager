# UpdateManager

**UpdateManager** is a self-updating service designed to handle automatic updates for desktop, console, and web applications. This project demonstrates how applications can check for updates from both local and remote sources, download, apply updates, and manage backup and restore functionality efficiently. It supports multiple deployment scenarios and comes with an example implementation for each type: console, desktop, and web.

Additionally, by customizing the `updateservice.json` file and utilizing **UpdateManager**, the application can automatically close if it is running, apply the update, and restart without requiring any additional code changes to the application itself. This allows you to launch your program via **UpdateManager**, ensuring that each time it checks for updates, downloads the latest version, and runs the updated application seamlessly.

## Features

- **Self-updating**: Automatically checks for updates from a specified URL or local file system path.
- **Cross-platform support**: Works for console applications, desktop applications, and ASP.NET MVC Core applications.
- **Backup and Restore**: Automatically creates backups of the current application version before applying updates.
- **Customizable**: Exclude files and directories from backup and deletion processes.
- **Production Artifacts Handling**: Ability to copy specific files or directories during the update process.
- **Version Checking**: Compares current application version against the latest available update and applies updates when available.
- **Local and Remote Update Source Support**: Checks for updates from a local file system, network share, or remote HTTP/HTTPS URLs.
- **Application Pool Management**: Automatically stops and starts the IIS application pool during the update process if the application is hosted on IIS.
- **Error handling and logging**: Logs detailed information during the update process, including errors.
- **Automatic Restart**: If the application is already running, it can automatically close, update itself, and restart without requiring any additional code changes. You can launch your program via **UpdateManager**, and it will handle update checks and running the latest version automatically.


## Project Structure

- **SelfUpdaterConsoleSample**: A sample console application that uses UpdateManager for self-updating.
- **SelfUpdaterDesktopSample**: A sample desktop application that prompts users to apply updates.
- **SelfUpdaterMvcCoreSample**: A sample ASP.NET Core MVC application that integrates the update check in a controller.
- **UpdateManager**: The core library that handles the update logic.
- **UpdateServices**: Contains configuration and utility services for managing the update process.

## Configuration

The `updateservice.json` file defines the settings for the update process, including:
```json
{
  "ExeFileName": "YourApp.exe",
  "UpdateCheckUrl": "https://example.com/updates/updateinfo.json",
  "LogsFolder": "C:\\path\\to\\logs",
  "UpdatesFolder": "C:\\path\\to\\updates",
  "BackupsFolder": "C:\\path\\to\\backups",
  "ApplicationFolder": "C:\\path\\to\\application",
  "ProductionArtifacts": ["artifact1", "artifact2"],
  "ExcludeFromBackup": ["file1.txt", "folder1"],
  "ExcludeFromDelete": ["file2.txt", "folder2"],
  "AutoStopApplication": true,
  "AutoRestartApplication": true,
  "ApplicationPoolName": "poolname"
}
```

### Configuration Details

- **ExeFileName**: The name of the application’s executable file.
- **UpdateCheckUrl**: URL or local path to the update information file (supports both remote and local sources).
- **LogsFolder**: Folder where log files will be stored. This can be an absolute or relative path. If left empty, an `Logs` folder will be created in the same directory where the executable is running.
- **ApplicationFolder**: The folder where the application is installed and run. This can be an absolute or relative path. If left empty, the application will use the folder where the executable is running.
- **UpdatesFolder**: Folder where update files will be downloaded and stored. This can be an absolute or relative path. If left empty, an `Updates` folder will be created in the same directory where the executable is running.
- **BackupsFolder**: Folder where backups of the current version will be saved. This can be an absolute or relative path. If left empty, a `Backups` folder will be created in the same directory where the executable is running.
- **ProductionArtifacts**: List of files or directories that should be copied after the update. Paths can be absolute or relative.
- **ExcludeFromBackup**: Files and directories to exclude from the backup process.
- **ExcludeFromDelete**: Files and directories to exclude from deletion during the update.
- **AutoStopApplication**: (true/false) Automatically stop the application before applying the update.
- **AutoRestartApplication**: (true/false) Automatically restart the application after applying the update.
- **ApplicationPoolName**: The name of the IIS application pool to stop and restart during the update process (if the application is hosted on IIS).

### Update Info Format

Update information can be provided either from a local path or a remote URL. Below are the examples of both formats:

#### Local Update Information (`updateinfo_local.json`)

```json
{
  "Version": "1.2.3",
  "DownloadUrl": "\\\\NetworkShare\\Updates\\app_1.2.3.zip"
}
```
#### Local Update Information (`updateinfo_remote.json`)

```json
{
  "Version": "1.2.3",
  "DownloadUrl": "https://example.com/downloads/app/app_1.2.3.zip"
}
```
In both cases, UpdateManager can check the current version and download the update from the specified location.


## How It Works

1. **Version Check**: The application checks the current version against the version specified in the update information file.
2. **Stop Application**: If `AutoStopApplication` is set to `true`, the application (or IIS application pool if configured) is automatically stopped before proceeding with the update.
3. **Download Update**: If a newer version is available, the update is downloaded to the `UpdatesFolder`.
4. **Backup**: Before applying the update, the current version is backed up (except those specified in `ExcludeFromBackup`).
5. **Delete Existing Files**: After the backup is completed, the existing files are deleted (except those specified in `ExcludeFromDelete`).
6. **Apply Update**: The downloaded update is extracted and applied to the `ApplicationFolder`.
7. **Copy Production Artifacts**: If there are any specified production artifacts, they are copied after the update has been applied.
8. **Restart Application**: If `AutoRestartApplication` is set to `true`, the application (or IIS application pool if configured) is automatically restarted after the update is applied.


## Examples

### Console Application Example
```csharp
Console.WriteLine("Checking for updates...");
var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
var config = UpdateServiceConfig.GetInstance("updateservice.json");

var latestUpdate = await updateChecker.CheckForUpdateAsync(config.UpdateCheckUrl, currentVersion);
if (latestUpdate != null)
{
    Console.WriteLine($"Update found! Version {latestUpdate.Version} is available.");
    Process.Start("UpdateManager.exe");
    Environment.Exit(0);
}
```
### MVC Core Application Example
```csharp
[HttpPost]
public async Task<IActionResult> CheckForUpdates()
{
    var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
    var config = UpdateServiceConfig.GetInstance("updateservice.json");

    var latestUpdate = await _updateChecker.CheckForUpdateAsync(config.UpdateCheckUrl, currentVersion!);
    return Json(new { updateAvailable = true, latestVersion=latestUpdate.Version });
}

[HttpGet]
public IActionResult ApplyUpdate()
{
    // Logic to apply the update (e.g., launch UpdateManager.exe)
    Process.Start("UpdateManager.exe");
    return RedirectToAction("Index", "Home");
}
```

In the MVC Core example, a basic web page could check for updates and trigger the update process with the following HTML and JavaScript:
```html
<div class="text-center">
    <h1 class="display-4">Welcome</h1>
    <p>Learn about <a href="https://learn.microsoft.com/aspnet/core">building Web apps with ASP.NET Core</a>.</p>
    <p>Version: @Assembly.GetExecutingAssembly().GetName().Version</p>
    <button id="btnCheckUpdates" class="btn btn-primary">Check for Updates</button>
</div>

<script>
    document.getElementById("btnCheckUpdates").addEventListener("click", function () {
        // Call the server to check for updates
        fetch('@Url.Action("CheckForUpdates", "Home")', {
            method: 'POST'
        })
        .then(response => response.json())
        .then(data => {
            if (data.updateAvailable) {
                if (confirm("Update found! Version " + data.latestVersion + " is available. Would you like to update now?")) {
                    window.location.href = '@Url.Action("ApplyUpdate", "Home")';
                }
            } else {
                alert("No updates found.");
            }
        })
        .catch(error => {
            alert("Error checking for updates: " + error);
        });
    });
</script>
```
### Desktop Application Example
```csharp
private async void btnCheckUpdates_Click(object sender, EventArgs e)
{
    var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
    var configFilePath = "updateservice.json";
    var config = UpdateServiceConfig.GetInstance(configFilePath);
    
    if (config == null)
        return;

    var latestUpdate = await _updateChecker.CheckForUpdateAsync(config.UpdateCheckUrl, currentVersion!);

    if (latestUpdate != null)
    {
        // Display a message box to the user, asking if they want to update
        if (MessageBox.Show(
            $"Update found! Version {latestUpdate.Version} is available. Would you like to update now?",
            "Update Available",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Information
        ) == DialogResult.Yes)
        {
            // Launch UpdateManager.exe to apply the update
            Process.Start("UpdateManager.exe");

            // Exit this process to allow the UpdateManager to complete the update
            Environment.Exit(0);
        }
    }
    else
    {
        MessageBox.Show("No updates found.");
    }
}
```

## How to Use

1. Clone the repository.
2. Customize the `updateservice.json` file for your application.
3. Build and integrate the **UpdateManager** into your application.
4. Set up your update information file on a local or remote server.
5. Start the update process via the provided sample applications.
6. Optionally, by customizing the `updateservice.json` file and calling **UpdateManager**, the application can be closed if it's running, updated, and restarted without any additional code changes. You can launch your program via **UpdateManager**, and each time it will check for updates, download the latest version, and run the updated application.
7. **For IIS-hosted applications**: Updates cannot be applied directly through the website while it’s running. In this case, **UpdateManager.exe** needs to be manually executed. The update process will stop the IIS application pool, apply the update, and restart the pool automatically if configured in the `updateservice.json` file.
8. **Manual Trigger Scenarios**: In scenarios where **UpdateManager** needs to be manually triggered, **UpdateManager.exe** does not need to be included in the project itself. It can be stored and executed from a separate folder. The update logic will still function as long as the correct paths are specified in the `updateservice.json` configuration.
9. **Handling Incomplete or Failed Updates**: In case of an incomplete or failed update, the application may need to be restored manually from the backup folder. The backups are located in the `BackupsFolder` defined in `updateservice.json`, and the desired backup can be manually copied to the `ApplicationFolder` to restore the previous version.



## License

This project is licensed under the MIT License.

