namespace MAUIApp;

public static class MauiProgram
{

    // _appConfig must be static so that those classes that are registered in the DI container can access it.
    // For a MAUI app, the appConfig.js is loaded from the embedded resource in the BlazorUI assembly.
    private static JObject? _appConfig;

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
        .UseMauiApp<App>()
        .ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
        });

        var isAndroid = false;
#if ANDROID
        isAndroid = true;
#endif

#if ANDROID && DEBUG
        //Platforms.Android.DangerousAndroidMessageHandlerEmitter.Register();
        //Platforms.Android.DangerousTrustProvider.Register();
#endif

        var isLocal = Debugger.IsAttached;
        var configText = BlazorUI.AssemblyContent.ReadEmbeddedResource("wwwroot/appConfig.js");
        _appConfig = ExtractDataFromJs(configText);

        // Configure logging
        builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug); // Set minimum log level
        builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);  // Only show Warning and above for ASP.NET Core


        // Here, we only register classes that require specific MAUI configuration.
        // The call to AddBlazorUI() will register all the other classes that are not MAUI specific.
        builder.Services.AddSingleton(sp => new HttpClient());

        builder.Services.AddSingleton<IStaticAssets>(sp => new BlazorStaticAssets(
            sp.GetRequiredService<ILoggerFactory>(),
            new HttpClient { BaseAddress = new Uri((string)_appConfig!["assetsUrl"]!) }));

        builder.Services.AddSingleton<ILzHost>(sp => new LzHost(
            appPath: (string)_appConfig!["appPath"]!, // app path
            appUrl: (string)_appConfig!["appUrl"]!, // app url  
            androidAppUrl: (string)_appConfig!["androidAppUrl"]!, // android app url 
            remoteApiUrl: (string)_appConfig!["remoteApiUrl"]!,  // api url
            localApiUrl: (string)_appConfig!["localApiUrl"]!, // local api url
            assetsUrl: (string)_appConfig!["assetsUrl"]!, // tenancy assets url
            authConfigName: (string)_appConfig!["authConfigName"]!, // auth config name
            isMAUI: true,
            isAndroid: isAndroid,
            isLocal: isLocal,
            useLocalhostApi: (bool)_appConfig!["useLocalHostApi"]!));

        builder.Services.AddMauiBlazorWebView(); ;

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        builder.Services.AddApp();

        // Add dynamic OIDC authentication with lazy-loaded configuration
        // This doesn't block startup waiting for config to load

        builder.Services.AddLazyMagicOIDCMAUI(); // Add services

        var host = builder.Build();

        // Load OIDC configuration in background (non-blocking)
        _ = Task.Run(async () => await ConfigureLazyMagicOIDCMAUI.LoadConfiguration(host));

        return host;
    }

    private static JObject? ExtractDataFromJs(string content)
    {
        string pattern = @"\{[^{}]*\}";
        Match match = Regex.Match(content, pattern);

        if (match.Success)
        {
            string jsonText = match.Value;
            JObject jsonObject = JObject.Parse(jsonText);
            return jsonObject;
        }
        else
        {
            return null;
        }
    }

}
