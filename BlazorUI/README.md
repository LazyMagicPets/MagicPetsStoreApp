# App 

This razor library contains the UI used by both the Store.WASM and Store.Maui projects.

### SessionsViewModel 
We use a singleton ViewModel to hold the current session(s) information. This is injected into the Index.razor page and it loads the connection and tenancy configuration.

We do not use a cascading value because we are inheriting from LzCoreComponent* classes that inject, or assign view models when instantiated. 

Note that this sample app only uses a single session at a time, but the framework provides for multiple sessions in the same process. Multi-sessions would be helpful for Point of Sale terminals running a MAUI app. A single instance of the app would be launched and each session would have it's own user. The user would log in with their normal username/password and then subsequent switching among sessions could be accomplished using a short pin. This is not demonstrated in this simple single user app, but the underlying framework will support this type of implementation.

### SessionsViewModel Initialization
There is subtle difference among MAUI Hybrid apps and Blazor apps that affects when the JSRuntime is available. If you try and use JSRuntime in Main.razor, NavMenu.razor or MainLayout.razor, it will fail with a JavaScript call outside web context error. 

### Init.razor page
This page is where we do configuration for the app. It is responsible for initializing the SessionsViewModel instance that was created in MainLayout.razor. Since the MainLayout.razor inherits from LzLayoutComponentInjectViewModel<ISessionsViewModel>, it is bound to properties like IsInitialized in that view model and can conditionally show layout content based on IsInitialized.

For single session apps, we also create the first ISessionViewModel in this page.

### Loading connection configuration 
We use the JSRuntime to call JavaScript functions load the connection configuration from Blazor _content. This means you can't use the connection configuration until you reach the Init.razor page.

### Loading the tenancy configuration 
Ditto for the tenancy configuration.
