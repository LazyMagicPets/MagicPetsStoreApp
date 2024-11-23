# Data Models

## Connection Flow
The table below shows the flow of messages between the client and the host for two concurrent clients, Joe and Sam. Joe is the first to connect and authenticate. Joe subscribes to the `Pet` topic. Sam connects and authenticates after Joe. Sam also subscribes to the `Pet` topic. 

Joe reads a pet from the host and then calls the host to update the `Pet` record. The update process writes a message to the notifications table. A NotificationsLambda is subscribed to the notifications table stream, and based on the Pet message and Sam's subscription to the topic `Pet`, sends a message to Sam and that message is used to update his copy of that `Pet` record. 

#### Joe logs in to `uptown.mydomain.com/storeapp`.

||Client|Flow|Message|Host|Table|Data|
|---|---|---|---|---|---|---|
|Joe|AuthProcess|->|UserName,Password|Cognito||
|Joe|AuthProcess|<-|JWT|Cognito|||
|Joe|StoreNotifications.Connect|->|Connect|WebSocketLambda|Subscription|ConnectionId|
|Joe|StoreNotifications|<-|ConnectionId|WebSocket|||
|Joe|StoreNotifications.Authenticate|->|Authenticate: JWT, SessionId|WebSocketLambda|Subscription|SessionId|
|Joe|StoreNotifications|<-|Success|WebSocketLambda|||
|Joe|StoreNotifications.Subscribe|->|Subscribe:Topics[Pet]|NotificationsLambda|Subscription|Topics[Pet]|

Sam logs in to `uptown.mydomain.com/storeapp` The same process as above is repeated for Sam. Note that each client has a unique ConnectionId and SessionId. The ConnectionId is assigned by the API Gateway WebSocket service and the SessionId is assigned by the Client app.

#### Joe and Sam list pets.

||Client|Flow|Message|Host|Table|Data|
|---|---|---|---|---|---|---|
|Joe|StoreLib.ListPets|-> All<br><- Pet Records||StoreLambda|uptown|Pet|
|Sam|StoreLib.ListPets|-> All<br><- Pet Records||StoreLambda|uptown|Pet|

#### Joe updates a pet record and Sam's client recieves a message to update its copy of that pet record.

||Client|Flow|Message|Host|Table|Data|
|---|---|---|---|---|---|---|
|Joe|StoreLib.UpdatePet|->|Pet<br>SessionIdHeader|StoreLambda|uptown|Pet|
|Joe||||StoreLambda|Notifications|Message: SessionId, Pet, Topics[Pet]|
|Joe|StoreLib.UpdatePet|<-|Pet|StoreLambda|uptown|Pet|
|Sam|StoreNotifications.ReceiveAsync|<-|Pet|NotificationsLambda<br>`Notifications table Stream Fires Lambda`|Subscriptions<br>Notifications|SetssionId, Topics[Pet]<br>Pet|

### Notes
The client makes only Connect, Authenticate and Subscribe messages to the host. 

Subscription records older than 5 seconds and which don't have a SessionId are deleted by the 
WebSocketLambda and the corrosponding connection is closed.

Since SessionIds are written only on authentication, this prevents connection flooding. 
Subscriptions messages require the client be authenticated.

### SessionIds
SessionIds identify a client instance. SessionIds are passed as a request header in all calls made to the API. The lazystack back end passes the SessionId to the process writing updating the message record in the notifications table. The SessionId is optional, but if present, the message is not sent back to the client that made the update to the Pet record that triggered the message. 

### Reconnection
When a client looses connection, it will reconnect and re-authenticate. Since, the SessionId will not have changed, the clients subscriptions will be re-activated. Essentually, the ConnectionId in the subscription record will be updated to the new ConnectionId associated with that SessionId. This avoids having the client resubscribe to the topics it had previously subscribed to.

As part of reconnection, the client can also call StoreNotificationsSvc.ReadNotificationsAsync to get any messages that were missed while the client was disconnected. The message records in the Notification table are available for some time (usually a few hours) after they are written. The Notifications table has TTL enabled and the records are automatically deleted after the TTL expires.

### Other uses for the Notifications Table
Since the Notifications table is a stream, it can be used to trigger other processes. For example, a process could be triggered to feed the data to a data warehouse or to send a message to a third party service.

