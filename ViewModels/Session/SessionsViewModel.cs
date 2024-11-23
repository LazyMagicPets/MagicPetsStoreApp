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
        ILoggerFactory loggerFactory,   
        ISessionViewModelFactory sessionViewModelFactory
        ) : base(loggerFactory)
    {
        _sessionViewModelFactory = sessionViewModelFactory;
    }
    private ISessionViewModelFactory _sessionViewModelFactory;

    public JObject TenancyConfig { get; set; } = new JObject();

    public override ISessionViewModel CreateSessionViewModel()
    {
        return _sessionViewModelFactory.Create();
    }

}
