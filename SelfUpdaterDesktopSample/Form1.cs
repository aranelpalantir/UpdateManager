using System.Diagnostics;
using System.Reflection;
using UpdateServices.Config;
using UpdateServices.Services;

namespace SelfUpdaterDesktopSample
{
    public partial class Form1 : Form
    {
        private readonly UpdateChecker _updateChecker = new();
        public Form1()
        {
            InitializeComponent();
        }
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

        private void Form1_Load(object sender, EventArgs e)
        {
            lblCurrentVersion.Text =Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
