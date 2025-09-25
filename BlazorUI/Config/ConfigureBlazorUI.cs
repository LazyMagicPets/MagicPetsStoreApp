namespace BlazorUI;

public static class ConfigureBlazorUI
{
    public static IServiceCollection AddBlazorUI(this IServiceCollection services)
    {
        services.AddBaseAppBlazorUI();
        return services;
    }
}
