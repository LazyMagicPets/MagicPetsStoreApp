// Import configurations and key modules
import { appConfig } from './_content/BlazorUI/appConfig.js';
import { settings } from './_content/BlazorUI/staticContentSettings.js';
import * as staticContentModule from './_content/BaseApp.BlazorUI/staticContentModule.js'; // Note namespace use
import { connectivityService } from './_content/LazyMagic.Blazor/connectivityService.js';
import { uIFetchLoadStaticAssets } from './_content/BaseApp.BlazorUI/UIFetch.js';

// Assign global references to import modules
window.appConfig = {
    appPath: appConfig.appPath,
    appUrl: window.location.origin,
    androidAppUrl: appConfig.androidAppUrl,
    remoteApiUrl: appConfig.remoteApiUrl,
    localApiUrl: appConfig.localApiUrl,
    assetsUrl: appConfig.assetsUrl
};
window.settings = settings;
window.staticContentModule = staticContentModule;
window.connectivityService = connectivityService;

// Kick off the static asset loading process
await uIFetchLoadStaticAssets(); // This will load the static assets into the cache(s)
