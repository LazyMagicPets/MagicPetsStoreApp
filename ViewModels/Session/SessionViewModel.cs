using Amazon;
using LazyMagic.Client.FactoryGenerator; // do not put in global using. Causes runtime error.

namespace ViewModels;
/// <summary>
/// The SessionViewModel is the root viewModel for a user session.
/// This class maintains the "state" of the user session, which includes 
/// the data (in this case the PetsViewMode).
/// </summary>Dep
[Factory]
public class SessionViewModel : BaseAppSessionViewModelAuthNotifications, ISessionViewModel
{
    public SessionViewModel(
        [FactoryInject] ILoggerFactory loggerFactory, // singleton
        [FactoryInject] ILzClientConfig clientConfig, // singleton
        [FactoryInject] IInternetConnectivitySvc internetConnectivity, // singleton
        [FactoryInject] ILzHost lzHost, // singleton
        [FactoryInject] ILzMessages messages, // singleton
        [FactoryInject] IAuthProcess authProcess, // transient
        [FactoryInject] IPetsViewModelFactory petsViewModelFactory, // transient
        [FactoryInject] ICategoriesViewModelFactory categoriesViewModelFactory, // transient
        [FactoryInject] ITagsViewModelFactory tagsViewModelFactory, // transient
        ISessionsViewModel sessionsViewModel
        )
        : base(loggerFactory, authProcess, clientConfig, internetConnectivity, messages)  
    {
        try
        {
            var tenantKey = (string?)clientConfig.TenancyConfig["tenantKey"] ?? throw new Exception("Cognito TenancyConfig.tenantKey is null");
            TenantName = AppConfig.TenantName;
            authProcess.SetAuthenticator(clientConfig.AuthConfigs?["TenantAuth"]!);
            authProcess.SetSignUpAllowed(false);

            var sessionId = Guid.NewGuid().ToString(); 

            Store = new StoreApi.StoreApi(new LzHttpClient(loggerFactory, authProcess.AuthProvider, lzHost, sessionId));

            Consumer = new ConsumerApi.ConsumerApi(new LzHttpClient(loggerFactory, authProcess.AuthProvider, lzHost, sessionId));

            Public = new PublicApi.PublicApi(new LzHttpClient(loggerFactory, null, lzHost, sessionId));

            PetsViewModel = petsViewModelFactory?.Create(this) ?? throw new ArgumentNullException(nameof(petsViewModelFactory));

            CategoriesViewModel = categoriesViewModelFactory?.Create(this) ?? throw new ArgumentNullException(nameof(categoriesViewModelFactory));

            TagsViewModel = tagsViewModelFactory?.Create(this) ?? throw new ArgumentNullException(nameof(tagsViewModelFactory));

        }
        catch (Exception ex)
        {
            Console.WriteLine($"SetAuthenticator failed. {ex.Message}");
            throw new Exception("oops");
        }
    }
    public IStoreApi Store { get; set; }
    public IConsumerApi Consumer { get; set; }
    public IPublicApi Public { get; set; }  

    public PetsViewModel PetsViewModel { get; set; }
    public CategoriesViewModel CategoriesViewModel { get; set; }
    public TagsViewModel TagsViewModel { get; set; }
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