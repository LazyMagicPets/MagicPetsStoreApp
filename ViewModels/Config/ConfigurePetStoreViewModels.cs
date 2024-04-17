namespace ViewModels;

public static class ConfigureStore
{
    public static IServiceCollection AddAppViewModels(this IServiceCollection services)
    {
        var assembly = MethodBase.GetCurrentMethod()?.DeclaringType?.Assembly;

        // Read MethodMap.json - this enables the LzHttpClient class to call the correct API hosting each method. Remember 
        // that a stack may host multiple APIGateways. 
        using var methodMapResource = typeof(IService).Assembly.GetManifestResourceStream("LZMServiceClientSDK.MethodMap.json")!;
        using var methodMapReader = new StreamReader(methodMapResource);
        var methodMapJson = methodMapReader.ReadToEnd();
        IMethodMapWrapper? methodMapWrapper = JsonConvert.DeserializeObject<MethodMapWrapper>(methodMapJson);
        LzViewModelFactory.RegisterLz(services, assembly!); // Register services having interfaces ILzTransient, ILzSingleton and ILzScoped

        services.AddSingleton<ILzClientConfig,LzClientConfig>();    
        services.AddLazyStackAuth();
        services.AddLazyStackViewModels();
        services.AddSingleton<ISessionsViewModel, SessionsViewModel>();
        services.TryAddSingleton(methodMapWrapper!);
        services.TryAddTransient<IService, Service>();
        services.TryAddTransient<ILzNotificationSvc, StoreNotificationSvc>();

        return services;
    }

    public static ILzMessages AddAppViewModels(this ILzMessages messages)
    {
        //var assembly = MethodBase.GetCurrentMethod()?.DeclaringType?.Assembly;
        var assembly = typeof(ConfigureStore).Assembly;

        var assemblyName = assembly!.GetName().Name;

        using var messagesStream = assembly.GetManifestResourceStream($"{assemblyName}.Config.Messages.json")!;
        // Add/Overwrite messages with messages in this library's Messages.json
        if (messagesStream != null)
        {
            using var messagesReader = new StreamReader(messagesStream);
            var messagesText = messagesReader.ReadToEnd();
            messages.MergeJson(messagesText);
        }
        return messages;
    }

}
