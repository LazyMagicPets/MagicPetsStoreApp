using Amazon;
using LazyMagic.Client.FactoryGenerator; // do not put in global using. Causes runtime error.

namespace ViewModels;
/// <summary>
/// The SessionViewModel is the root viewModel for a user session.
/// This class maintains the "state" of the user session, which includes 
/// the data (in this case the PetsViewMode).
/// </summary>
[Factory]
public class SessionViewModel : LzSessionViewModelAuthNotifications, ISessionViewModel
{
    public SessionViewModel(
        [FactoryInject] ILoggerFactory loggerFactory, // singleton
        [FactoryInject] ILzClientConfig clientConfig, // singleton
        [FactoryInject] IInternetConnectivitySvc internetConnectivity, // singleton
        [FactoryInject] ILzHost lzHost, // singleton
        [FactoryInject] ILzMessages messages, // singleton
        [FactoryInject] IAuthProcess authProcess, // transient
        [FactoryInject] IPetsViewModelFactory petsViewModelFactory, // transient
        [FactoryInject] IStoreNotificationSvcFactory storeNotificationsSvcFactory // transient
        )
        : base(loggerFactory, authProcess, clientConfig, internetConnectivity, messages)  
    {
        try
        {
            //var lzHostRpt = JsonConvert.SerializeObject(lzHost, new JsonSerializerSettings { Formatting = Formatting.Indented });   
            //Console.WriteLine(lzHostRpt);

            var tenantKey = (string?)clientConfig.TenancyConfig["tenantKey"] ?? throw new Exception("Cognito TenancyConfig.tenantKey is null");
            var sessionId = Guid.NewGuid().ToString(); 

            var securityLevelStr = (string?)clientConfig.AuthConfigs?["TenantAuth"]?["userPoolSecurityLevel"] ?? throw new Exception("Cognito AuthConfig.securityLevel is null");
            var securityLevel = int.Parse(securityLevelStr);

            ILzHttpClient httpClientStore = new LzHttpClient(loggerFactory, securityLevel, authProcess.AuthProvider, lzHost, sessionId);
            Store = new StoreApi.StoreApi(httpClientStore);

            ILzHttpClient httpClientConsumer = new LzHttpClient(loggerFactory, securityLevel,  authProcess.AuthProvider, lzHost, sessionId);
            Consumer = new ConsumerApi.ConsumerApi(httpClientConsumer);

            ILzHttpClient httpClientPublic = new LzHttpClient(loggerFactory, 0, null, lzHost, sessionId);
            Public = new PublicApi.PublicApi(httpClientPublic);

            //NotificationsSvc = storeNotificationsSvcFactory.Create(authProcess, internetConnectivity, sessionId, Store);

            this.petsViewModelFactory = petsViewModelFactory ?? throw new ArgumentNullException(nameof(petsViewModelFactory));
            PetsViewModel = petsViewModelFactory.Create(this);
            TenantName = AppConfig.TenantName;
            var _region = (string?)clientConfig.Region ?? throw new Exception("Cognito AuthConfig.region is null");
            var regionEndpoint = RegionEndpoint.GetBySystemName(_region);
            authProcess.SetAuthenticator(clientConfig.AuthConfigs?["TenantAuth"]!);

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
        if(PetsViewModel != null)
            PetsViewModel = petsViewModelFactory.Create(this);
        await Task.Delay(0);    
    }
}