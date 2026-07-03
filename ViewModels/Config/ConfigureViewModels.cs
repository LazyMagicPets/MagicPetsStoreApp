using LazyMagic.Shared;

namespace ViewModels;

public static class ConfigureViewModels
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {

        ViewModelsRegisterFactories.ViewModelsRegister(services); // Register Factory Classes

        // Register the ClientSDK 
        services.AddScoped<IAppApi>(serviceProvider =>
        {
            var lzHost = serviceProvider.GetRequiredService<ILzHost>();
            var authenticationHandler = serviceProvider.GetRequiredService<IAuthenticationHandler>();
            var handler = authenticationHandler.CreateHandler();
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(lzHost.GetApiUrl("")) // LocalApiUrl or RemoteApiUrl depending on UseLocalhostApi property
            };
            var api = new AppApi.AppApi(httpClient);
            // Blazor WASM forbids SYNCHRONOUS reads on the HTTP response stream. NSwag's default
            // ReadObjectResponseAsync path (ReadResponseAsString=false) deserializes straight from
            // the response Stream synchronously → every AppApi call throws
            // net_http_synchronous_reads_not_supported (the StoreApp data layer was dead in WASM).
            // Forcing the string path makes it read the body async (ReadAsStringAsync) first.
            api.ReadResponseAsString = true;
            return api;
        });

        // Register the modules used from the Client SDK.
        services.AddScoped<IPublicModuleClient>(provider => provider.GetRequiredService<IAppApi>());
        services.AddScoped<IConsumerModuleClient>(provider => provider.GetRequiredService<IAppApi>());
        services.AddScoped<IStoreModuleClient>(provider => provider.GetRequiredService<IAppApi>());

        services.AddScoped<ISessionViewModel, SessionViewModel>();
        services.AddTransient<IBaseAppSessionViewModel>(sp => sp.GetRequiredService<ISessionViewModel>());

        services.AddBaseAppViewModels();

        return services;
    }
}

