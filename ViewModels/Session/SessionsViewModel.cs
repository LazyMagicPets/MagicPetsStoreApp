using System.Reactive.Disposables;
namespace ViewModels;
/// <summary>
/// Manage user session. This is the root viewModel for the application. It is passed to and accessible 
/// from most components. It keeps track of the current user session. Note that this class is 
/// a singleton created by the DI container.
/// Call the InitAsync() method before calling other methods in the class. 
/// </summary>
public class SessionsViewModel : LzSessionsViewModelAuthNotifications<ISessionViewModel>, ISessionsViewModel
{
    public SessionsViewModel(
        ILzMessages messages,
        ISessionViewModelFactory sessionViewModelFactory
        )  : base(messages)
    {
        _sessionViewModelFactory = sessionViewModelFactory;
    }
    private ISessionViewModelFactory _sessionViewModelFactory;

    public JObject TenancyConfig { get; set; } = new JObject();

    public override ISessionViewModel CreateSessionViewModel()
    {
        return _sessionViewModelFactory.Create(OSAccess, ClientConfig!, InternetConnectivity!);
    }

    // ReadConfigAsync is called from InitAsync() just prior to the IsInitialized being set to true.
    public override async Task ReadConfigAsync()
    {
        await base.ReadConfigAsync();
        await Messages.SetMessageSetAsync(new LzMessageSet("en-US", LzMessageUnits.Imperial));
    }
}
