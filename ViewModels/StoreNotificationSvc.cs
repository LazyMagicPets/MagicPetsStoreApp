
namespace ViewModels;

public class StoreNotificationSvc : LzNotificationSvc, ILzNotificationSvc
{
    public StoreNotificationSvc(
        ISessionViewModel sessionViewModel,
        ILzClientConfig clientConfig,
        ILzHost lzHost, 
        IInternetConnectivitySvc internetConnectivitySvc
        ) : base(clientConfig, lzHost, sessionViewModel.AuthProcess, internetConnectivitySvc)
    {
        store = sessionViewModel.Store;   
        this.internetConnectivity = internetConnectivitySvc;

        this.WhenAnyValue(x => x.internetConnectivity.IsOnline,x => x.authProcess.IsSignedIn)
            .Subscribe(async x =>
            {
                if (x.Item1 && x.Item2)
                    await ConnectAsync();
            });

        this.WhenAnyValue(x => x.internetConnectivity.IsOnline, x => x.authProcess.IsSignedIn, (x, y) => x && y)
            .Do(x => Console.WriteLine($"yada: {x}"));
    }
    private IService store;
    public override async Task<List<LzNotification>> ReadNotificationsAsync(string connectionId,  long lastDateTimeTick)
    {
        var notifications = new List<LzNotification>();
        var more = false;
        do
        {
            var notificationsResult = await store.LzNotificationsPageListSessionIdDateTimeTicksAsync(connectionId, lastDateTimeTicks);
            more = notificationsResult.More;
        } while (more);

        return notifications;
    }
    public override Task<(bool success, string msg)> SubscribeAsync(List<string> topicIds)
    {
        return Task.FromResult((true, "ok"));
    }
    public override Task<(bool success, string msg)> UnsubscribeAsync(List<string> topicIds)
    {
        return Task.FromResult((true, "ok"));
    }
    public override Task<(bool success, string msg)> UnsubscribeAllAsync()
    {
        return Task.FromResult((true, "ok"));
    }

}
