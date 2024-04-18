

namespace BlazorUI;

public static class ConfigureBlazorUI
{
    public static IServiceCollection AddBlazorUI(this IServiceCollection services)
    {
        //var assembly = MethodBase.GetCurrentMethod()?.DeclaringType?.Assembly;
        var assembly = typeof(ConfigureBlazorUI).Assembly;
        ILzMessages messages = new LzMessages();
        // Load in order of override. Messages in the last loaded file
        // will override messages in the first loaded file.
        messages
            .AddLazyStackComponents()
            .AddLazyStackViewModels()
            .AddLazyStackAuth()
            .AddAppViewModels()
            .AddApp()
            .ReplaceVars();

        return services
            .AddSingleton<ILzMessages>(messages)
            .AddAppViewModels()
            .AddLazyStackComponents()
            .AddLazyStackAuthCognito();
    }

    public static ILzMessages AddApp(this ILzMessages messages)
    {
        //var assembly = MethodBase.GetCurrentMethod()?.DeclaringType?.Assembly;
        var assembly = typeof(ConfigureBlazorUI).Assembly;

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
