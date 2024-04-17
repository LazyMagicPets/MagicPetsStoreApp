using LazyStack.Client.Base;
using BlazorUI;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

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

        var localHost = false; // flip this to true to hit the local host api

        var apiUrl = string.Empty;  
        var tenancyUrl = string.Empty;  

        if( Debugger.IsAttached )
        {
            apiUrl = localHost
                ? (isAndroid ? "http://localhost:5011" : "https://localhost:5001")
                : "https://uptown.lazymagicdev.click";
            tenancyUrl = "https://uptown.lazymagicdev.click";
        }
        else 
        {
            // The CI/CD pipeline is responsible for writing the hosturl.json file to the app package
            // Resources/Raw folder. We don't put this file in wwwroot/Tenancy because recoruces in
            // that folder may be retrieved from the host url instead of deployed with the app.
            using var stream = FileSystem.OpenAppPackageFileAsync("hosturl.json").Result;    
            using var reader = new StreamReader(stream);
            var contents = reader.ReadToEnd();
            apiUrl = tenancyUrl = JObject.Parse(contents)["hosturl"]!.ToString();    
        }

        builder.Services
        .AddSingleton(sp => new HttpClient())
        .AddSingleton<ILzHost>(sp => new LzHost(url: apiUrl, tenancyUrl: tenancyUrl, isMAUI: true, isAndroid: isAndroid))
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
