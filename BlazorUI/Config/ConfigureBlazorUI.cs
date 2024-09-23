

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
            "System/{culture}/System/AuthMessages.json",
            "System/{culture}/System/BaseMessages.json",
            "System/{culture}/StoreApp/Messages.json",
            ];
        lzMessages.MessageFiles.AddRange(messages);
        return lzMessages;
    }
}
