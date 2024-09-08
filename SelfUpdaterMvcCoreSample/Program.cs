using System.Diagnostics;
using UpdateServices.Services;

var builder = WebApplication.CreateBuilder(args);

#if !DEBUG
if (IsRunningUnderIIS())
{
    // Do nothing, since it's running under IIS
}
else
{
    var httpUrl = builder.Configuration.GetSection("Kestrel:Endpoints:Http:Url").Value;
    var httpsUrl = builder.Configuration.GetSection("Kestrel:Endpoints:Https:Url").Value;

    builder.WebHost.UseKestrel(options =>
    {
        if (!string.IsNullOrEmpty(httpUrl))
        {
            options.ListenAnyIP(new Uri(httpUrl).Port);
        }
      
        if (!string.IsNullOrEmpty(httpsUrl))
        {
            options.ListenAnyIP(new Uri(httpsUrl).Port, listenOptions =>
            {
                listenOptions.UseHttps();
            });
        }
    });
}
#endif

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<UpdateChecker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

#if !DEBUG
if (IsRunningUnderIIS())
{
    // Do nothing, since it's running under IIS
}
else
{
    OpenBrowser("http://localhost:5000");
}
#endif



app.Run();

bool IsRunningUnderIIS()
{
    return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APP_POOL_ID"));
}

void OpenBrowser(string url)
{
    try
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error occurred while opening the browser: {ex.Message}");
    }
}
