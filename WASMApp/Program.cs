namespace WASMApp;

public class Program
{
    private static JObject? _appConfig;
    public static async Task Main(string[] args)
    {
        // ReactiveUI 23.x removed automatic initialization (20.x auto-initialized
        // via the RxApp static ctor). It must now be initialized explicitly via the
        // builder before any WhenAnyValue / [Reactive] usage — and the LazyMagic /
        // BaseApp ViewModel base classes call WhenAnyValue in their constructors,
        // which run during the first component render. Without this call the first
        // reactive access throws TypeInitializationException on
        // ReactiveNotifyPropertyChangedMixin and the app fails to boot.
        LazyMagic.Blazor.LzReactiveUI.InitializeWasm();

        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<Main>("#main");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        // We use the launchSettings.json profile ASPNETCORE_ENVIRONMENT environment variable
        // to determine the host addresses for the API host and Tenant host.
        //
        // Examples:
        // Production: "ASPNETCORE_ENVIRONMENT": "Production" 
        //  The API and Tenant host are the same and are the base address of the cloudfront distribution
        //  the app is loaded from.
        //
        // Debug against LocalHost API:
        //  "ASPNETCORE_ENVIRONMENT": "Localhost"
        //  useLocalhostApi will be true else false

        var hostEnvironment = builder.HostEnvironment;
        var apiUrl = string.Empty;
        var assetsUrl = string.Empty;
        var isLocal = false; // Is the code being served from a local development host?
        var useLocalhostApi = false;
        switch (hostEnvironment.Environment)
        {
            case "Production":
                Console.WriteLine("Loaded from CloudFront");
                builder.Logging.SetMinimumLevel(LogLevel.Warning);
                break;
            default:
                Console.WriteLine("Development environment");
                builder.Logging.SetMinimumLevel(LogLevel.Information);
                isLocal = true;
                var envVar = hostEnvironment.Environment;
                if (envVar.Contains("Localhost"))
                    useLocalhostApi = true;
                break;
        }

        // Configure logging
        builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug); // Set minimum log level
        builder.Logging.AddFilter("Microsoft.AspNetCore",
            LogLevel.Warning); // Only show Warning and above for ASP.NET Core

        builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri((string)_appConfig!["assetsUrl"]!) });

        builder.Services.AddSingleton<IStaticAssets>(sp => new BlazorStaticAssets(
                sp.GetRequiredService<ILoggerFactory>(),
                new HttpClient { BaseAddress = new Uri((string)_appConfig!["assetsUrl"]!) }));

        builder.Services.AddSingleton<ILzHost>(sp =>
            new LzHost(
                appPath: (string)_appConfig!["appPath"]!, // web app path
                appUrl: (string)_appConfig!["appUrl"]!, // web app url
                androidAppUrl: (string)_appConfig!["androidAppUrl"]!, // android app url 
                remoteApiUrl: (string)_appConfig!["remoteApiUrl"]!,  // api url
                localApiUrl: (string)_appConfig!["localApiUrl"]!, // local api url
                assetsUrl: (string)_appConfig!["assetsUrl"]!, // tenancy assets url
                authConfigName: (string)_appConfig!["authConfigName"]!, // auth config name
                isMAUI: false, // sets isWASM to true
                isAndroid: false,
                isLocal: isLocal,
                useLocalhostApi: useLocalhostApi));

        builder.Services.AddApp();

        // Auth-mode selection (ADDITIVE, opt-in). Config key "AuthMode" in
        // wwwroot/appsettings.json: "Bff" enables the client-side BFF mode;
        // anything else (incl. absent) keeps the existing default SPA-token path
        // EXACTLY as-is. Clean if/else so the default branch is byte-for-byte unchanged.
        var authMode = builder.Configuration["AuthMode"];
        var useBff = string.Equals(authMode, "Bff", StringComparison.OrdinalIgnoreCase);

        if (useBff)
        {
            // BFF mode: the SPA holds no tokens. The BffCredentialsHandler (registered
            // as IAuthenticationHandler below) replaces the BearerTokenHandler, so the
            // existing IAppApi wiring in ConfigureViewModels builds a cookie-credentialed,
            // same-origin client (credentials:include + X-CSRF:1) with no further change.
            // The /bff/* endpoints are reached relative to the WASM host's base address.
            Console.WriteLine("AuthMode=Bff: using client-side BFF auth");
            // Employee-gated app: after logout, land on the public /explore/home/ landing (the BFF
            // server fans it back to the originating subtenant host) instead of bouncing back through
            // the gated store to the login.
            builder.Services.AddLazyMagicOIDCWASMBff(
                builder.HostEnvironment.BaseAddress,
                postLogoutRedirectPath: "/explore/home/");
        }
        else
        {
            // Default SPA-token path — UNCHANGED.
            builder.Services.AddTransient<IAuthenticationHandler, BearerTokenHandler>();

            // Add dynamic OIDC authentication with lazy-loaded configuration
            // This doesn't block startup waiting for config to load
            builder.Services.AddLazyMagicOIDCWASM(); // Add services
            builder.AddLazyMagicOIDCWASMBuilder(); // Add builder configuraiton

            // LOCAL VS-DEBUG ONLY: pin the OIDC redirect_uri to THIS WASM's own
            // origin (e.g. https://localhost:7218). The cloud /config serves
            // RedirectUri=<apex>/oauth2/callback — the single Cognito-registered
            // callback shared by every subtenant — and relies on CFAuthCallback
            // fanning the OAuth response back to the originating subtenant via the
            // wrapped `state`. That fan-back targets the /config host (the cloud
            // subtenant), NEVER localhost, so a VS-hosted WASM would complete login
            // on the cloud host and this local instance's authorize state would be
            // orphaned (observed: "Found stale tokens ... cleaning up"). Redirecting
            // straight back to the localhost origin fixes it — the dev callbacks
            // https://localhost:7218/authentication/{login,logout}-callback are
            // registered on the SPA client (systemconfig IncludeDevCallbackUrls;
            // lz AwsAppRunnerCognitoComponent). This PostConfigure runs AFTER
            // DynamicOidcPostConfigureOptions (registered inside AddLazyMagicOIDCWASM)
            // so it wins. Guarded by isLocal → cloud (isLocal=false, and BFF anyway)
            // is byte-for-byte unaffected.
            if (isLocal)
            {
                var localOrigin = builder.HostEnvironment.BaseAddress.TrimEnd('/');
                builder.Services.PostConfigure<RemoteAuthenticationOptions<OidcProviderOptions>>(options =>
                {
                    options.ProviderOptions.RedirectUri = $"{localOrigin}/authentication/login-callback";
                    options.ProviderOptions.PostLogoutRedirectUri = $"{localOrigin}/authentication/logout-callback";
                });
            }
        }

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

        // SPA-token OIDC config load. Skipped in BFF mode (no SPA OIDC services
        // are registered there; the BFF provider needs no client-side discovery).
        if (!useBff)
        {
            await ConfigureLazyMagicOIDCWASM.LoadConfiguration(host);
        }

        await host.RunAsync();

    }

    private static async Task LoadStaticAssets(IJSRuntime jsRuntime)
    {
        await jsRuntime.InvokeVoidAsync("loadStaticAssets");
    }

    private static async Task<JObject> GetAppConfigAsync(IJSRuntime jsRuntime)
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