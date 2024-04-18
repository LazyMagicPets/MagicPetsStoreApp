namespace WASMApp;
public partial class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.RootComponents.Add<Main>("#main");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        // We use the launchSettings.json profile ASPNETCORE_ENVIRONMENT environment variable
        // to determine the host addresses for the API host and Tenancy host.
        //
        // Examples:
        // Production: "ASPNETCORE_ENVIRONMENT": "Production" 
        //  The API and Tenancy host are the same and are the base address of the cloudfront distribution
        //  the app is loaded from.
        //
        // Debug against LocalHost API:
        //  "ASPNETCORE_ENVIRONMENT": "https://localhost:5001,https://admin.lazymagicdev.click"
        //  The first url is the API host, the second is the tenancy host.
        //
        // Debug against CloundFront deployment:
        //   "ASPNETCORE_ENVIRONMENT": "https://admin.lazymagicdev.click"
        //   The url is the API host and the tenancy host.


        var hostEnvironment = builder.HostEnvironment;
        var apiUrl = string.Empty;
        var tenancyUrl = string.Empty;  
        var isLocal = false; // Is the code being served from a local development host?
        switch(hostEnvironment.Environment)
        {
            case "Production":
                Console.WriteLine("Production environment");
                apiUrl = tenancyUrl = builder.HostEnvironment.BaseAddress;
                break;
            default:
                Console.WriteLine("Development environment");
                var envVar = hostEnvironment.Environment;
                // The variable is a list of urls, separated by a comma.
                var urls = envVar.Split(',');
                apiUrl = urls[0];
                apiUrl = apiUrl.EndsWith('/') ? apiUrl : apiUrl + '/';
                tenancyUrl = urls.Length > 1 ? urls[1] : urls[0];
                tenancyUrl = tenancyUrl.EndsWith('/') ? tenancyUrl : tenancyUrl + '/';
                Console.WriteLine($"apiUrl: {apiUrl}");
                Console.WriteLine($"tenancyUrl: {tenancyUrl}");
                isLocal = true; 
                break;
        }

        builder.Services
            .AddSingleton(sp => new HttpClient { BaseAddress = new Uri(apiUrl) })
            .AddSingleton<ILzHost>(sp => new LzHost(
                url: apiUrl,  // api url
                tenancyUrl: tenancyUrl, // tenancy assets url
                isMAUI: false, // sets isWASM to true
                isAndroid: false, 
                isLocal: isLocal))
            .AddBlazorUI(); // See Config/ConfigureViewModels.cs

        await builder.Build().RunAsync();
    }
}