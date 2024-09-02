using Amazon;


namespace ViewModels;
/// <summary>
/// The SessionViewModel is the root viewModel for a user session.
/// This class maintains the "state" of the use session, which includes 
/// the data (in this case the PetsViewMode).
/// </summary>
[Factory]
public class SessionViewModel : LzSessionViewModelAuthNotifications, ISessionViewModel, ILzTransient
{
    public SessionViewModel(
        [FactoryInject] ILzClientConfig clientConfig, // singleton
        [FactoryInject] IInternetConnectivitySvc internetConnectivity, // singleton
        [FactoryInject] ILzHost lzHost, // singleton
        [FactoryInject] ILzMessages messages, // singleton
        [FactoryInject] IAuthProcess authProcess, // transient
        [FactoryInject] IPetsViewModelFactory petsViewModelFactory // transient
        )
        : base( authProcess, clientConfig, internetConnectivity, messages)  
    {
        try
        {
            var tenantKey = (string?)clientConfig.TenantKey ?? throw new Exception("Cognito AuthConfig.tenantKey is null");

            var securityLevelStr = (string?)clientConfig.AuthConfigs?["EmployeeAuth"]?["userPoolSecurityLevel"] ?? throw new Exception("Cognito AuthConfig.securityLevel is null");
            var securityLevel = int.Parse(securityLevelStr);

            ILzHttpClient httpClientStore = new LzHttpClient(securityLevel, tenantKey, authProcess.AuthProvider, lzHost);
            Store = new StoreApi.StoreApi(httpClientStore);

            ILzHttpClient httpClientConsumer = new LzHttpClient(securityLevel, tenantKey, authProcess.AuthProvider, lzHost);
            Consumer = new ConsumerApi.ConsumerApi(httpClientConsumer);

            ILzHttpClient httpClientPublic = new LzHttpClient(0, tenantKey, null, lzHost);
            Public = new PublicApi.PublicApi(httpClientPublic);

            NotificationsSvc = new StoreNotificationSvc(this, clientConfig, lzHost, internetConnectivity);
            this.petsViewModelFactory = petsViewModelFactory ?? throw new ArgumentNullException(nameof(petsViewModelFactory));
            PetsViewModel = petsViewModelFactory.Create(this);
            TenantName = AppConfig.TenantName;
            var _region = (string?)clientConfig.Region ?? throw new Exception("Cognito AuthConfig.region is null");
            var regionEndpoint = RegionEndpoint.GetBySystemName(_region);
            authProcess.SetAuthenticator(clientConfig.AuthConfigs?["EmployeeAuth"]!);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SetAuthenticator failed. {ex.Message}");
            throw new Exception("opps");
        }
    }
    public IStoreApi Store { get; set; }
    public IConsumerApi Consumer { get; set; }
    public IPublicApi Public { get; set; }  

    private IPetsViewModelFactory petsViewModelFactory;  
    public PetsViewModel PetsViewModel { get; set; }
    public string TenantName { get; set; } = string.Empty;

    // Base class calls LoadAsync () when IsSignedIn changes to true
    public override async Task LoadAsync()
    {
        await PetsViewModel.ReadAsync();
    }
    // Base class calls UnloadAsync () when IsSignedIn changes to false
    public override async Task UnloadAsync()
    {
        PetsViewModel = petsViewModelFactory.Create(this);
        await Task.Delay(0);    
    }
}