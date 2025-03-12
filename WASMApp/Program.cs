
using LazyMagic.Client.ViewModels;
using ReactiveUI.Blazor;


namespace WASMApp;
public partial class Program
{
    private static JObject? _appConfig;

    private static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.RootComponents.Add<Main>("#main");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        // We use the launchSettings.json profile ASPNETCORE_ENVIRONMENT environment variable
        // to determine the host addresses for the API host and Assets host.
        //
        // Examples:
        // Production: "ASPNETCORE_ENVIRONMENT": "Production" 
        //  The API and Assets host are the same and are the base address of the cloudfront distribution
        //  the app is loaded from.
        //
        // Debug against LocalHost API:
        //  "ASPNETCORE_ENVIRONMENT": "Localhost"
        //  useLocalhostApi will be true else false


        var hostEnvironment = builder.HostEnvironment;
        var isLocal = false; // Is the code being served from a local development host?
        var useLocalhostApi = false;
        switch (hostEnvironment.Environment)
        {
            case "Production":
                Console.WriteLine("Loaded from CloudFront");
                break;
            default:
                Console.WriteLine("Development environment");
                isLocal = true;
                var envVar = hostEnvironment.Environment;
                if (envVar.Contains("Localhost"))
                    useLocalhostApi = true;
                break;
        }

        // Configure logging
        builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug); // Set minimum log level
        builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);  // Only show Warning and above for ASP.NET Core
        builder.Logging.AddFilter("MudBlazor", LogLevel.Warning);  // Only show Warning and above for MudBlazor

        builder.Services
        
        .AddSingleton(sp => new HttpClient { BaseAddress = new Uri((string)_appConfig!["assetsUrl"]!) })
        .AddSingleton<IStaticAssets>(sp => new BlazorStaticAssets(
            sp.GetRequiredService<ILoggerFactory>(), 
            new HttpClient { BaseAddress = new Uri((string)_appConfig!["assetsUrl"]!) }))
        .AddSingleton<ILzMessages, LzMessages>()
        .AddSingleton<ILzClientConfig, LzClientConfig>()
        .AddSingleton<BlazorInternetConnectivity>()

        .AddSingleton<IBlazorInternetConnectivity>(sp => sp.GetRequiredService<BlazorInternetConnectivity>())
        .AddSingleton<IInternetConnectivitySvc>(sp => sp.GetRequiredService<BlazorInternetConnectivity>())
        .AddSingleton<ILzHost>(sp => new LzHost(
            appPath: (string)_appConfig!["appPath"]!, // app path
            appUrl: (string)_appConfig!["appUrl"]!, // app url  
            androidAppUrl: "", // android app url not used in WASM
            remoteApiUrl: (string)_appConfig!["remoteApiUrl"]!,  // api url
            localApiUrl: (string)_appConfig!["localApiUrl"]!, // local api url
            assetsUrl: (string)_appConfig!["assetsUrl"]!, // tenancy assets url
            isMAUI: false, // sets isWASM to true
            isAndroid: false,
            isLocal: isLocal,
            useLocalhostApi: useLocalhostApi))
        .AddSingleton<IOSAccess, BlazorOSAccess>()
        .AddSingleton<IBaseAppJS, BaseAppJS>()
        .AddBlazorUI(); // See Config/ConfigureViewModels.cs


        var host = builder.Build();


        // Wait for the page to fully load to finish up the Blazor app configuration
        var jsRuntime = host.Services.GetRequiredService<IJSRuntime>();
        await WaitForPageLoad(jsRuntime);

        // Now we can retrieve the app config information loaded with the page
        _appConfig = await GetAppConfigAsync(jsRuntime);
        if (_appConfig == null)
        {
            Console.WriteLine("Error loading app config. Exiting.");
            return;
        }

        await host.RunAsync();
    }

    private static async Task LoadStaticAssets(IJSRuntime jsRuntime)
    {
        await jsRuntime.InvokeVoidAsync("loadStaticAssets");
    }

    private static async Task<JObject?> GetAppConfigAsync(IJSRuntime jsRuntime)
    {
        try
        {
            // Use IJSRuntime to evaluate JavaScript and get the JSON string
            string jsonString = await jsRuntime.InvokeAsync<string>(
                "eval",
                "JSON.stringify(window.appConfig)"
            );

            // Parse the JSON string to a JObject
            return JObject.Parse(jsonString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching app config: {ex.Message}");
            return null;
        }
    }

    private static async Task WaitForPageLoad(IJSRuntime jsRuntime)
    {
        const int maxWaitTimeMs = 10000; // Maximum wait time of 10 seconds
        const int checkIntervalMs = 100; // Check every 100ms

        var totalWaitTime = 0;
        while (totalWaitTime < maxWaitTimeMs)
        {
            var isLoaded = await jsRuntime.InvokeAsync<bool>("checkIfLoaded");
            if (isLoaded)
            {
                Console.WriteLine("Page fully loaded.");
                return;
            }

            await Task.Delay(checkIntervalMs);
            totalWaitTime += checkIntervalMs;
        }

        Console.WriteLine("Warning: Page load timeout reached.");
    }

}