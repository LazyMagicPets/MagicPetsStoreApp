

using MudBlazor.Services;

namespace BlazorUI;

public static class ConfigureBlazorUI
{
    public static IServiceCollection AddBlazorUI(this IServiceCollection services)
    {
        return services
            .AddMudServices()
            .AddAppViewModels()
            .AddLazyMagicAuthCognito();
    }
    public static ILzMessages AddBlazorUIMessages(this ILzMessages lzMessages)
    {
        List<string> messages = [
            "Assets/{culture}/System/AuthMessages.json",
            "Assets/{culture}/System/BaseMessages.json",
            "Assets/{culture}/StoreApp/Messages.json",
            ];
        lzMessages.MessageFiles.AddRange(messages);
        return lzMessages;
    }
}
