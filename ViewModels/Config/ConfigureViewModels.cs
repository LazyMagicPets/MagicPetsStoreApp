namespace ViewModels;

public static class ConfigureViewModels
{
    public static IServiceCollection AddAppViewModels(this IServiceCollection services)
    {
        var assembly = MethodBase.GetCurrentMethod()?.DeclaringType?.Assembly;

        LzViewModelFactory.RegisterLz(services, assembly!); // Register services having interfaces ILzTransient, ILzSingleton and ILzScoped

        services.AddSingleton<ILzClientConfig,LzClientConfig>();    
        services.AddLazyMagicAuthCognito();
        services.AddSingleton<ISessionsViewModel, SessionsViewModel>();
        services.TryAddTransient<IStoreApi, StoreApi.StoreApi>();
        services.TryAddTransient<IConsumerApi, ConsumerApi.ConsumerApi>();
        services.TryAddTransient<IPublicApi, PublicApi.PublicApi>();
        services.TryAddTransient<ILzNotificationSvc, StoreNotificationSvc>();

        return services;
    }

}
