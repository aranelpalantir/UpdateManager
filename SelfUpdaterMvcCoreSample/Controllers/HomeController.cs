using Microsoft.AspNetCore.Mvc;
using SelfUpdaterMvcCoreSample.Models;
using System.Diagnostics;
using System.Reflection;
using UpdateServices.Config;
using UpdateServices.Services;

namespace SelfUpdaterMvcCoreSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UpdateChecker _updateChecker;

        public HomeController(ILogger<HomeController> logger, UpdateChecker updateChecker)
        {
            _logger = logger;
            _updateChecker = updateChecker;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> CheckForUpdates()
        {
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

            var configFilePath = "updateservice.json";
            var config = UpdateServiceConfig.GetInstance(configFilePath);
            if (config == null)
            {
                return Json(new { updateAvailable = false });
            }

            var latestUpdate = await _updateChecker.CheckForUpdateAsync(config.UpdateCheckUrl, currentVersion!);

            if (latestUpdate != null)
            {
                // Update is available, notify the client
                return Json(new { updateAvailable = true, latestVersion=latestUpdate.Version });
            }
            else
            {
                // No update available
                return Json(new { updateAvailable = false });
            }
        }

        [HttpGet]
        public IActionResult ApplyUpdate()
        {
            // Logic to apply the update (e.g., launch UpdateManager.exe)

            // This would usually trigger the update process (such as stopping the app, etc.)
            Process.Start("UpdateManager.exe");

            // Redirect back to the home page or another view while the update is applied
            return RedirectToAction("Index", "Home");
        }
    }
}
