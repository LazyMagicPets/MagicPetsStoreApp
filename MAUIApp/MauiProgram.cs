using LazyMagic.Client.Base;
using BlazorUI;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using LazyMagic.Blazor;
using ViewModels;
using System.Text.RegularExpressions;
namespace MAUIApp;

public static class MauiProgram
{
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
        builder.Logging.AddFilter("MudBlazor", LogLevel.Warning);  // Only show Warning and above for MudBlazor

        builder.Services
            .AddSingleton<ILzMessages, LzMessages>()
            .AddSingleton<ILzClientConfig, LzClientConfig>()
            .AddSingleton(sp => new HttpClient())
            .AddSingleton<IStaticAssets>(sp => new BlazorStaticAssets(
                sp.GetRequiredService<ILoggerFactory>(),
                new HttpClient { BaseAddress = new Uri((string)_appConfig!["assetsUrl"]!) }))
            .AddSingleton<BlazorInternetConnectivity>()
            .AddSingleton<IBlazorInternetConnectivity>(sp => sp.GetRequiredService<BlazorInternetConnectivity>())
            .AddSingleton<IInternetConnectivitySvc>(sp => sp.GetRequiredService<BlazorInternetConnectivity>())
            .AddSingleton<ILzHost>(sp => new LzHost(
                appPath: (string)_appConfig!["appPath"]!, // app path
                appUrl: (string)_appConfig!["appUrl"]!, // app url  
                androidAppUrl: (string)_appConfig!["androidAppUrl"]!, // android app url 
                remoteApiUrl: (string)_appConfig!["remoteApiUrl"]!,  // api url
                localApiUrl: (string)_appConfig!["localApiUrl"]!, // local api url
                assetsUrl: (string)_appConfig!["assetsUrl"]!, // tenancy assets url
                isMAUI: true,
                isAndroid: isAndroid,
                isLocal: isLocal,
                useLocalhostApi: (bool)_appConfig!["useLocalHostApi"]!))
            .AddSingleton<IOSAccess, BlazorOSAccess>()
            .AddSingleton<IBaseAppJS, BaseAppJS>()
            .AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif
        builder.Services.AddBlazorUI();
        return builder.Build();
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
