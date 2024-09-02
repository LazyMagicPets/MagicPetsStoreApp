using LazyMagic.Client.Base;
using BlazorUI;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using LazyMagic.Blazor;

namespace MAUIApp;

public static class MauiProgram
{
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
        var isLocal = false;// flip this to true to hit the local host api
        var cloudHost = "https://uptown.lazymagicdev.click/";
        var apiUrl = string.Empty;  
        var assetsUrl = string.Empty;
        var wsUrl = string.Empty;
        if ( Debugger.IsAttached )
        {
            apiUrl = isLocal
                ? (isAndroid ? "http://localhost:5011" : "https://localhost:5001")
                : cloudHost;
            assetsUrl = cloudHost;
            assetsUrl = cloudHost;
            wsUrl = apiUrl.Replace("https", "wss").Replace("http", "ws") + "ws";
        }
        else
        {
            // The CI/CD pipeline is responsible for writing the hosturl.json file to the app package
            // Resources/Raw folder. We don't put this file in wwwroot/Tenancy because recoruces in
            // that folder may be retrieved from the host url instead of deployed with the app.
            using var stream = FileSystem.OpenAppPackageFileAsync("hosturl.json").Result;    
            using var reader = new StreamReader(stream);
            var contents = reader.ReadToEnd();
            apiUrl = assetsUrl = JObject.Parse(contents)["hosturl"]!.ToString();
            wsUrl = assetsUrl.Replace("https", "wss").Replace("http", "ws") + "ws";
        }

        builder.Services
            .AddSingleton<ILzMessages, LzMessages>()
            .AddSingleton<ILzClientConfig, LzClientConfig>()
            .AddSingleton(sp => new HttpClient())
            .AddSingleton<IStaticAssets>(sp => new BlazorStaticAssets(new HttpClient { BaseAddress = new Uri(assetsUrl) }))
            .AddSingleton<BlazorInternetConnectivity>()
            .AddSingleton<IBlazorInternetConnectivity>(sp => sp.GetRequiredService<BlazorInternetConnectivity>())
            .AddSingleton<IInternetConnectivitySvc>(sp => sp.GetRequiredService<BlazorInternetConnectivity>())
            .AddSingleton<ILzHost>(sp => new LzHost(
                url: apiUrl, 
                assetsUrl: assetsUrl, 
                isMAUI: true, 
                isAndroid: isAndroid,
                isLocal: isLocal))
            .AddSingleton<IOSAccess, BlazorOSAccess>()
            .AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif
        builder.Services
        .AddBlazorUI();
        return builder.Build();
    }
}
