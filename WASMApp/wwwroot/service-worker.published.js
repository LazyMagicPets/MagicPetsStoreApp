/*
This service-worker is used when the app is running from a remote server.
In addition to the minimum set of service-worker features, this implementation 
includes support for:
- Integration with our static asset caching module.
- Redirection to the base URL when the app is navigated to a different URL.
- Graceful application updates.
*/
import * as staticContentModule from './_content/BlazorUI/staticContentModule.js'; // staticAssets
import * as appConfigFile from './_content/BlazorUI/appConfig.js'; // appConfig
const appPrefix = appConfigFile.appConfig.appPath;

const swversion = 48;
console.log("service-worker.js loading version:" + swversion);

let version = '';

async function sendMessage(action, info) {
    const clients = await self.clients.matchAll({ type: 'window', includeUncontrolled: true });
    if (clients) {
        for (const client of clients) {
            client.postMessage({
                action: action,
                info: info
            });
        }
    }
}

const offlineAssetsInclude = [/\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/, /\.svg$/];
const offlineAssetsExclude = [/^service-worker\.js$/, /GoogleTag\.js$/]; // Excluding GoogleTag.js because ad blockers block it and this will cause the caching to fail.

self.addEventListener('message', async event => {
    switch (event.data.action) {
        case 'checkForNewAssetData':
            console.log('service worker checkForNewAssetData.');
            await sendMessage('AssetDataCheckStarted', "");
            break;
        case 'loadStaticAssets':
            console.log('service worker loadStaticAssets.');
            await caches.keys().then(cacheNames => {
                console.log('Available caches:', cacheNames);
            });
            await staticContentModule.readAssetCachesByType("PreCache");
            break;
        case 'listCaches':
            caches.keys().then(cacheNames => {
                console.log('Available caches:', cacheNames);
            });
            break;
        default:
            break;
    }
    if (event.data.type === 'SET_ASSET_HOST_URL') {
        try {
            self.assetHostUrl = new URL(event.data.url);
        } catch (error) {
            console.error(error);
        }
    }
});


self.addEventListener('install', event => {
    console.info('Service worker installing...' + swversion);
    event.waitUntil(
        (async () => {
            try {
                const response = await fetch('./service-worker-assets.js');
                const content = await response.text();
                eval(content);

                // Let the main UI thread know there is a new service worker installing
                //const clients = await navigator.serviceWorker?.controller.clients.matchAll({ type: 'window' });
                await sendMessage('ServiceWorkerUpdateStarted', 'A new version is being installed.');

                // Fetch and cache all matching items from the assets manifest into the temp cache
                version = self.assetsManifest.version;
                console.log('assetRequests.length():' + self.assetsManifest.assets.length + ", version:" + version);
                const assetsRequests = self.assetsManifest.assets
                    .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
                    .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
                    .map(asset => {
                        if (asset.url.includes('index.html')) {
                            // We don't us asset.hash on index.html because we update the base tag after the 
                            // msbuild publish step.
                            return new Request(asset.url, {
                                cache: 'no-cache'
                            });
                        }
                        else {
                            return new Request(asset.url, {
                                integrity: asset.hash,
                                cache: 'no-cache'
                            });
                        }
                    });

                await staticContentModule.cacheApplicationAssets(assetsRequests);
                self.skipWaiting(); // Activate the new service worker immediately
            } catch (error) {
                console.error('Error during service worker install:', error);
            }

        })()
    );
});

// This event listener is used to make sure all existing clients are claimed by the new service worker
self.addEventListener('activate', (event) => {
    console.log("Service worker activating: " + swversion);
    event.waitUntil((async () => {
        await staticContentModule.activateApplicationCache();
        await self.clients.claim();
        await staticContentModule.readAssetCachesByType("PreCache");
        await sendMessage("ServiceWorkerUpdateCompleted", "The new version has been installed.");
    })());
});

self.addEventListener('fetch', event => {
    event.respondWith((async () => {

        // Redirect to the base URL if the app is navigated to a different URL
        if (event.request.mode === 'navigate') {
            const url = new URL(event.request.url);
            const redirectUrl = new URL(appPrefix, self.location.origin);
            if (url.pathname !== redirectUrl.pathname) {
                console.log(`Redirecting from ${url.pathname} to ${redirectUrl.pathname}`);
                return Response.redirect(redirectUrl.href, 302);
            }
        }

        if (event.request.method === 'GET' && event.request.cache !== "no-cache") {
            try {
                // examine the request path and determin if this may be a cached asset
                const cacheName = await staticContentModule.getCacheName(event.request.url);
                if (cacheName) {
                    const cacheStatus = await checkCacheStatus();
                    if (cacheStatus) {
                        await staticContentModule.lazyLoadAssetCache(cacheName);
                        const cachedResponse = await staticContentModule.getCachedResponse(cacheName, event.request);
                        if (cachedResponse instanceof Response) {
                            return cachedResponse;
                        } else {
                            // Item is not in cache so just fetch it. We don't add it to the cache here because of
                            // thread safety issues. This is not a performance issue becuase the browser's native
                            // cache will have the item, for the cache load to use, when the cache load catches up.
                            return fetch(event.request);
                        }
                    }
                    else {
                        console.warn('No caches found when processing:' + event.request.url);
                    }
                }
            }
            catch (error) {
                console.error('Error during fetch:', event.request.url, error);
                return new Response('Error during fetch: ', { status: 500 });
            }
        }
        // we don't need await on the fetch because it returns a promise
        return fetch(event.request);
    })()
    );
});

async function checkCacheStatus() {
    const cacheNames = await caches.keys();
    return {
        exist: cacheNames.length > 0,
        count: cacheNames.length
    };
}

