namespace ViewModels;

public interface ISessionsViewModel : ILzSessionsViewModelAuthNotifications<ISessionViewModel> {
    public JObject TenancyConfig { get; set; }
}

