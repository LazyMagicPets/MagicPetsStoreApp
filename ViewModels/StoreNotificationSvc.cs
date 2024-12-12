
namespace ViewModels;
using LazyMagic.Client.FactoryGenerator; // do not put in global using. Causes runtime error.

public class WebSocketMessage
{
    public string Action { get; set; } = "";
    public JsonElement Data { get; set; } = new();
}

public class AuthenticateAction
{
    public string Token { get; set; } = "";
    public string SessionId { get; set; } = "";
}

public class AuthenticationSuccessAction
{
    public string ConnectionId { get; set; } = "";
}


[Factory]
public class StoreNotificationSvc : LzViewModel, ILzNotificationSvc, IAsyncDisposable
{ 
    public StoreNotificationSvc(
        [FactoryInject] ILoggerFactory loggerFactory,   
        [FactoryInject] ILzClientConfig clientConfig,
        [FactoryInject] ILzHost lzHost,
        IAuthProcess authProcess,
        IInternetConnectivitySvc internetConnectivitySvc,
        string sessionId,
        IStoreApi storeApi
        ) : base(loggerFactory) 
    {
        AuthProcess = authProcess;
        InternetConnectivitySvc = internetConnectivitySvc;
        this.clientConfig = clientConfig;
        this.lzHost = lzHost;
        websocketUrl = new Uri(lzHost.WsUrl);   
        this.sessionId = sessionId ?? "";
        this.storeApi = storeApi;

        this.WhenAnyValue(x => x.AuthProcess.IsSignedIn)
            .Throttle(TimeSpan.FromMilliseconds(100))
            .DistinctUntilChanged()
            .Subscribe(async x =>
            {
                if(x)
                    await EnsureConnectedAsync();
                
                if(!x)
                    await DisconnectAsync();
            });

    }
    IStoreApi storeApi = null!;

    protected ClientWebSocket? ws;
    protected Uri websocketUrl;
    protected ILzClientConfig clientConfig;
    protected string connectionId = string.Empty;
    protected ILzHost lzHost;
    protected string sessionId;
    protected long lastDateTimeTicks = 0L;
    private LzNotification? _notification;
    public LzNotification? Notification
    {
        get { return _notification; }
        set { this.RaiseAndSetIfChanged(ref _notification, value); }
    }
    protected bool isBusy = false;
    protected string createdAtFieldName = "CreatedAt";
    public bool Debug { get; set; } = false;    
    public IAuthProcess AuthProcess { get; init; }
    public IInternetConnectivitySvc InternetConnectivitySvc { get; init; }
    [Reactive] public bool IsActive { get; set; }
    public ObservableCollection<string> Topics { get; set; } = new ObservableCollection<string>();
    LzNotification? ILzNotificationSvc.Notification { get; set; }

    public async ValueTask DisposeAsync()
    {
        _logger.LogDebug("NotificationSvc.DisposeAsync");
        await DisconnectAsync(); // calls ws.Dispose()  
        GC.SuppressFinalize(this);
    }

    public async Task SendAsync(string message)
    {
        if (ws is null)
        {
            _logger.LogDebug($"WebSocket SendAsync failed. WebSocketClient is null");
            return;
        }

        if (ws.State != WebSocketState.Open)
        {
            _logger.LogDebug($"WebSocket SendAsync failed. State={ws.State}");
            return;
        }

        message = "{\"action\": \"message\", \"content\": \"{message}\"}";

        var bytes = Encoding.UTF8.GetBytes(message);

        await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

        _logger.LogDebug($"WebSocket SendAsync done. State={ws.State}");
    }


    public async Task<List<LzNotification>> ReadNotificationsAsync(string connectionId,  long lastDateTimeTick)
    {   
        var notifications = new List<LzNotification>();
        //var more = false;
        //do
        //{
        //    var notificationsResult = await storeApi.LzNotificationsPageListSessionIdDateTimeTicksAsync(connectionId, lastDateTimeTicks);
        //    more = notificationsResult.More;
        //} while (more);

        return notifications;
    }

    public  async Task EnsureConnectedAsync()
    {
        //Todo: Clean up the handling of the websocket lifecycle. 
        ws ??= new ClientWebSocket();

        _logger.LogDebug($"EnsureConnectedAsync. WebSocketState={ws.State}");

        if (ws.State == WebSocketState.Open || ws.State == WebSocketState.Connecting)
            return;

        if (ws.State == WebSocketState.Closed || ws.State == WebSocketState.Aborted)
        {
            ws.Dispose();
            ws = new ClientWebSocket();
        }

        if (ws.State != WebSocketState.None)
        {
            ws.Dispose();
            return;
        }

        try
        {
           _logger.LogDebug("Calling ws.ConnectAsync");
            await ws.ConnectAsync(websocketUrl, CancellationToken.None);
            await SendAuthenticationMessageAsync();
            await ListenForMessages();
        }
        catch (Exception ex)
        {
            var msg = "EnsureCoonectedAsync error: " + ex.Message;  
            _logger.LogDebug(msg);
        }
    }

    private async Task SendAuthenticationMessageAsync()
    {
        _logger.LogDebug("SendAuthenticationMessageAsync");
        try
        {
            var currentToken = await AuthProcess.GetAccessToken();

            var authenticationAction = new AuthenticateAction
            {
                Token = currentToken!,
                SessionId = sessionId
            };

            var authMessage = new WebSocketMessage
            {
                Action = "AUTHENTICATE",
                Data = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(System.Text.Json.JsonSerializer.Serialize(authenticationAction))
            };

            var jsonMessage = System.Text.Json.JsonSerializer.Serialize(authMessage);

            var bytes = Encoding.UTF8.GetBytes(jsonMessage);

            await ws!.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

            _logger.LogDebug("Authentication message sent");
        }
        catch (Exception ex)
        {
            _logger.LogDebug($"Error sending authentication message: {ex.Message}");
            throw;
        }
    }
    protected async Task ListenForMessages()
    {
        _logger.LogDebug("Listening for web socket messages");
        var buffer = new byte[1024];
        if (ws is null)
            return;
        while (ws != null && ws.State == WebSocketState.Open)
        {

            var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            _logger.LogDebug($"Received message. type:{result.MessageType}");
            switch (result.MessageType)
            {
                case WebSocketMessageType.Close:
                    _logger.LogDebug("WebSocket Close");
                    IsActive = false;
                    ws?.Dispose();
                    ws = null;
                    break;
                case WebSocketMessageType.Text:
                    var messageText = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var webSocketMessage = System.Text.Json.JsonSerializer.Deserialize<WebSocketMessage>(messageText);
                    if (webSocketMessage == null)
                    {
                        _logger.LogDebug($"Error processing websocket message.");
                        continue;
                    }

                    var action = webSocketMessage.Action;
                    var data = webSocketMessage.Data;
                    switch (action)
                    {
                        case "AUTHENTICATESUCCESS":
                            var connectionID = data.GetProperty("ConnectionId").GetString();
                            IsActive = true;
                            _logger.LogDebug($"Successfully Authenticated on ConnectionId: {connectionID}");
                            break;
                        case "NOTIFICATION":
                            var notification = System.Text.Json.JsonSerializer.Deserialize<LzNotification>(webSocketMessage.Data.GetRawText());
                            _logger.LogDebug($"Notification received: {notification}");
                            break;
                        default:
                            _logger.LogDebug($"Unknown action: {action}");
                            break;
                    }
                    break;
                case WebSocketMessageType.Binary:
                    _logger.LogDebug($"WebSocket Binary message.");
                    break;
            }
        }
        _logger.LogDebug("Exiting ListenForMessages");
    }

    public async Task ConnectAsync()
    {
        _logger.LogDebug("NotificationSvc.ConnectAsync()");
        await EnsureConnectedAsync();
    }
    public  Task<(bool success, string msg)> SubscribeAsync(List<string> topicIds)
    {
        return Task.FromResult((true, "ok"));
    }
    public  Task<(bool success, string msg)> UnsubscribeAsync(List<string> topicIds)
    {
        return Task.FromResult((true, "ok"));
    }
    public  Task<(bool success, string msg)> UnsubscribeAllAsync()
    {
        return Task.FromResult((true, "ok"));
    }

    public async Task DisconnectAsync()
    {
        IsActive = false;
        if (ws is null) return;

        if (ws.State == WebSocketState.Open)
        {
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            _logger.LogDebug($"DisconnectAsync called. ws.State={ws.State.ToString()}");
        }
    }

}
