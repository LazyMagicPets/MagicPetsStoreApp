import * as assetsCache from './_content/BlazorUI/staticContentModule.js'; // staticAssets
//import { assetCaches, fetchAssetCacheList } from './_content/BlazorUI/staticContentModule.js'; // staticAssets

(async function () {
    const originalFetch = fetch;
    console.log("Installing override fetch");

    console.log("calling assetsCache.fetchAssetsCacheList()");
    await assetsCache.fetchAssetCacheList();
    //for (const key of Object.keys(assetsCache.assetCaches))
    //    console.log("key:" + key);

    window.fetch = async function (...args) {

        const assetHostUrl = (typeof window.assetHostUrl === "string")
            ? new URL(window.assetHostUrl)
            : window.assetHostUrl;

        async function modifyUrl(originalUrl) {
            let url = new URL(originalUrl);
            url.hostname = assetHostUrl.hostname;
            url.port = assetHostUrl.port;
            return url.href;
        }

        try {
            let request = args[0];
            let options = args[1] || {};

            if (typeof request === 'string')
                request = new Request(request);

            let staticAsset = false;

            for (const curl of Object.keys(assetsCache.assetCaches)) {
                if (request.url.includes(curl)) {
                    staticAsset = true;
                    break;
                }
            }

            //console.log("request.url " + request.url);

            if (staticAsset) {

                if (!assetHostUrl) {
                    console.error("Asset host URL is not set. Trying to fetch:" + request.url);
                    return await originalFetch(request); // Default fetch if URL is not set properly
                }
                const newUrl = await modifyUrl(request.url);
                request = new Request(newUrl, {
                    method: request.method,
                    headers: request.headers,
                    mode: request.mode,
                    credentials: request.credentials,
                    redirect: request.redirect
                });
            }
            return await originalFetch(request, options);
        } catch (error) {
            console.error(error);
        }

        // Perform the actual fetch
        const response = await originalFetch.apply(this, args);
        return response;
    };
})();
