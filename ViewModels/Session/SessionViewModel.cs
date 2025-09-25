using LazyMagic.Client.FactoryGenerator; // do not put in global using. Causes runtime error.

namespace ViewModels;
/// <summary>
/// The SessionViewModel is the root viewModel for a user session.
/// This class maintains the "state" of the user session, which includes 
/// the data (in this case the PetsViewMode).
/// </summary>Dep
[Factory]
public class SessionViewModel : BaseAppSessionViewModel, ISessionViewModel
{
    public SessionViewModel(
        [FactoryInject] ILoggerFactory loggerFactory, // singleton
        [FactoryInject] ILzClientConfig clientConfig, // singleton
        [FactoryInject] IConnectivityService connectivityService, // singleton
        [FactoryInject] ILzHost lzHost, // singleton
        [FactoryInject] ILzMessages messages, // singleton
        [FactoryInject] IPetsViewModelFactory petsViewModelFactory, // transient
        [FactoryInject] ICategoriesViewModelFactory categoriesViewModelFactory, // transient
        [FactoryInject] ITagsViewModelFactory tagsViewModelFactory // transient
        )
        : base(loggerFactory,  connectivityService, messages,
                petsViewModelFactory, categoriesViewModelFactory, tagsViewModelFactory)  
    {
        try
        {
            TenantName = AppConfig.TenantName;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SetAuthenticator failed. {ex.Message}");
            throw new Exception("oops");
        }
    }

    public string TenantName { get; set; } = string.Empty;

    // Base class calls UnloadAsync () when IsSignedIn changes to false
    public override async Task UnloadAsync()
    {
        if (PetsViewModel != null) PetsViewModel.Clear();

        if (CategoriesViewModel != null) CategoriesViewModel.Clear();

        if (TagsViewModel != null) TagsViewModel.Clear();

        await Task.Delay(0);    
    }
}