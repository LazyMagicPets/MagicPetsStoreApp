
//using System.Net.WebSockets;
//using System.Text.Json;

//namespace ViewModels;

//public class WebSocketMessage
//{
//    public string Action { get; set; } = "";
//    public AuthenticateAction Data { get; set; } = new();
//}

//public class AuthenticateAction
//{
//    public string Token { get; set; } = "";
//    public string SessionId { get; set; } = "";
//}


//[Factory]
//public class StoreNotificationSvc : LzNotificationSvc, ILzNotificationSvc
//{
//    public StoreNotificationSvc(
//        [FactoryInject] ILzClientConfig clientConfig,
//        [FactoryInject] ILzHost lzHost,
//        IAuthProcess authProcess,
//        IInternetConnectivitySvc internetConnectivitySvc,
//        string sessionId,
//        IStoreApi storeApi
//        ) : base(clientConfig, lzHost, authProcess, internetConnectivitySvc, sessionId)
//    {
//        this.storeApi = storeApi;
//        Debug = true;
//    }
//    IStoreApi storeApi = null!;


//    public override async Task<List<LzNotification>> ReadNotificationsAsync(string connectionId,  long lastDateTimeTick)
//    {   
//        var notifications = new List<LzNotification>();
//        var more = false;
//        do
//        {
//            var notificationsResult = await storeApi.LzNotificationsPageListSessionIdDateTimeTicksAsync(connectionId, lastDateTimeTicks);
//            more = notificationsResult.More;
//        } while (more);

//        return notifications;
//    }
//    public override async Task EnsureConnectedAsync()
//    {
//        //await base.EnsureConnectedAsync();
//        //Todo: Clean up the handling of the websocket lifecycle. 
//        ws ??= new ClientWebSocket();

//        if (Debug)
//            Console.WriteLine($"EnsureConnectedAsync. WebSocketState={ws.State}");

//        if (ws.State == WebSocketState.Open || ws.State == WebSocketState.Connecting)
//            return;

//        if (ws.State == WebSocketState.Closed || ws.State == WebSocketState.Aborted)
//            ws = new ClientWebSocket();

//        if (ws.State != WebSocketState.None)
//            return;

//        var uri = new Uri(lzHost.WsUrl);
//        try
//        {
//            if (Debug)
//                Console.WriteLine("Calling ws.ConnectAsync");
//            await ws.ConnectAsync(uri, CancellationToken.None);
//            await ListenForMessages();
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine(ex.Message.ToString());
//        }

//        var token = await AuthProcess.GetJWTAsync();

//        var authenticateAction = new AuthenticateAction { Token = token!, SessionId = sessionId };
//        var messageObject = new WebSocketMessage
//        {
//            Action = "AUTHENTICATE",
//            Data = authenticateAction
//        };

//        var message = JsonConvert.SerializeObject(messageObject);

//        await ws.SendAsync(
//            new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(message)), 
//            WebSocketMessageType.Text, 
//            true,
//            CancellationToken.None);
//    }

//    public override Task<(bool success, string msg)> SubscribeAsync(List<string> topicIds)
//    {
//        return Task.FromResult((true, "ok"));
//    }
//    public override Task<(bool success, string msg)> UnsubscribeAsync(List<string> topicIds)
//    {
//        return Task.FromResult((true, "ok"));
//    }
//    public override Task<(bool success, string msg)> UnsubscribeAllAsync()
//    {
//        return Task.FromResult((true, "ok"));
//    }

//}
