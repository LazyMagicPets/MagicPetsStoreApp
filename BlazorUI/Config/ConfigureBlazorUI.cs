

using MudBlazor.Services;

namespace BlazorUI;

public static class ConfigureBlazorUI
{
    public static IServiceCollection AddBlazorUI(this IServiceCollection services)
    {
        return services
            .AddBaseApp()
            .AddAppViewModels()
            ;
    }
    public static ILzMessages AddBlazorUIMessages(this ILzMessages lzMessages)
    {
        List<string> messages = [
            "system/{culture}/System/AuthMessages.json",
            "system/{culture}/System/BaseMessages.json",
            "system/{culture}/StoreApp/Messages.json",
            ];
        lzMessages.MessageFiles.AddRange(messages);
        return lzMessages;
    }
}
