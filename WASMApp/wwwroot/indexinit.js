/**
 * This module is used to initialize the Blazor WebAssembly app.
 * Use a link in your index.html file to import this module:
 * <script type="module" src="indexinit.js"></script>
 * 
 * This module is NOT used in the MAUI Hybrid app. 
 * The WASM app can run in three modes:
 * - localhost development calling the cloud apiUrl (calls UIFetch.js)
 * - localhost development calling the localApiUrl (calls UIFetch.js)
 * - cloud deployment (registers service-worker.js)
 * 
 * For localhost development, the app code is served by the localhost server. However, 
 * the app calls the cloud for static assets. The app may make service calls against either
 *  the cloud application or the localhost application api.
 * The websocket connection is used to process messages from the cloud. Since these messages 
 * are generally the result of data being written to the cloud database, we always connect
 * the development environment to the cloud websocket.
 */

window.isLoaded = false;

window.checkIfLoaded = function () {
    return window.isLoaded;
};

const baseHrefElement = document.querySelector('base');
const appPath = new URL(baseHrefElement.href).pathname;


if (window.location.origin.includes("localhost")) {
    /*** APP LOADED FROM THE LOCALHOST ***/
    console.debug("Running from local development host");
    const { appConfig } = await import('./_content/BlazorUI/appConfig.js');
    window.appConfig = {
        appPath: appConfig.appPath,
        appUrl: window.location.origin,
        androidAppUrl: "",
        remoteApiUrl: appConfig.remoteApiUrl,
        localApiUrl: appConfig.localApiUrl,
        assetsUrl: appConfig.assetsUrl
    };

    const { uIFetchLoadStaticAssets } = await import('./_content/BlazorUI/UIFetch.js');

    window.isLoaded = true; // This lets the app know it can proceed with the Blazor app startup

    await uIFetchLoadStaticAssets(); // This will load the static assets into the cache(s)

} else {
    /**** APP LOADED FROM NON-DEV HOST (cloud, remote host etc.) ****/
    // When runing from the cloud, the baseHref is set to the base URL of the app.
    // We do not set localApiUrl because the app has no access to localhost.
    console.debug("Running from cloud or remote server");
    // We are not using anything from appConfig.js when using a service worker. 
    // const { appConfig } = await import('./_content/BlazorUI/appConfig.js');

    // Note that the base href is updated on publish by a target in our
    // csproj file. It is difficult to set it dynamically as it is used
    // in the index.html file before we have a chance to modify it 
    // with a script.
    window.appConfig = {
        appPath: appPath,
        appUrl: window.location.origin + "/",
        androidAppUrl: "",
        remoteApiUrl: window.location.origin + "/",
        localhostApiUrl: "",
        assetsUrl: window.location.origin + "/",
        wsUrl: window.location.origin.replace(/^http/, 'ws') + "/"
    };

    // This lets the app know it can proceed with the Blazor app startup
    // We need to wait for the appConfig data to be gathered before we proceed. As you can
    // see in the appCofig, this data is gathered from the page load process, not from 
    // the appConfig.js file.
    window.isLoaded = true;

    // Note that the service worker activate event kicks off the asset caching process.
    navigator.serviceWorker.register('service-worker.js', { type: 'module', scope: appPath });
  

}
