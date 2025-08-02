using LazyMagic.Shared;
using System.Reactive.Disposables;
namespace ViewModels;
/// <summary>
/// Manage user session. This is the root viewModel for the application. It is passed to and accessible 
/// from most components. It keeps track of the current user session. Note that this class is 
/// a singleton created by the DI container.
/// Call the InitAsync() method before calling other methods in the class. 
/// </summary>
public class SessionsViewModel : BaseAppSessionsViewModel<ISessionViewModel>, ISessionsViewModel
{
    public SessionsViewModel(
        ILoggerFactory loggerFactory,
        ILzHttpClient lzHttpClient, // singleton
        ITenantConfigViewModelFactory tenantConfigViewModelFactory,
        IStaticAssets staticAssets,
        ILzJsUtilities lzJsUtilities,
        ISessionViewModelFactory sessionViewModelFactory
        ) : base(loggerFactory, tenantConfigViewModelFactory, staticAssets, lzJsUtilities)
    {
        _sessionViewModelFactory = sessionViewModelFactory;
        IsInitialized = true;
        lzHttpClient.Initialize(this);
    }
    private ISessionViewModelFactory _sessionViewModelFactory;


    public override ISessionViewModel CreateSessionViewModel()
    {
        var sessionViewModel = _sessionViewModelFactory.Create(this);
        return sessionViewModel;
    }
}
