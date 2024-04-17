// This service-worker.js file is used in development mode. It fetches code from the network and does not enable offline support.
// In a published app, this service-worker.js file is replaced by the service-worker.published.js file.
//
// Code:
// In development, always fetch code from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the first load after each change).
//
// Tenancy Data:
// To handle tenancy buckets in the dev environment, we intercept tenancy data requests
// using the 'fetch' event listener and retrieve them from the data host. Note that we are not
// caching these assets in development mode.
//
// In a published app, this service-worker.js file is replaced by the service-worker.published.js file.
// Both the code and tenancy assets are cached in a production app by service-worker.published.js.

self.tenancyHostUrl = null;
self.tenancyHostUrlPromise = new Promise((resolve, reject) => {
    self.resolveTenancyHostUrl = resolve;
});

self.addEventListener('message', async event => {
    if (event.data.type === 'SET_TENANCY_HOST_URL') {
        try {
            self.tenancyHostUrl = new URL(event.data.url);
            self.resolveTenancyHostUrl();
        } catch (error) {
            console.error(error);
        }
    }
});

self.addEventListener('fetch', async event => {
    const urlPattern = /_content\/Tenancy\//;

    async function modifyUrl(originalUrl) {
        await self.tenancyHostUrlPromise;  // Wait for the URL to be set
        let url = new URL(originalUrl);
        url.hostname = self.tenancyHostUrl.hostname;
        url.port = self.tenancyHostUrl.port;
        return url.href;
    }

    try {

        if (urlPattern.test(event.request.url)) {
            event.respondWith((async () => {
                if (!self.tenancyHostUrl) {
                    console.error("Tenancy host URL is not set.");
                    return await fetch(event.request); // Default fetch if URL is not set properly
                }
                const newUrl = await modifyUrl(event.request.url);

                const modifiedRequest = new Request(newUrl, {
                    method: event.request.method,
                    headers: event.request.headers,
                    mode: event.request.mode,
                    credentials: event.request.credentials,
                    redirect: event.request.redirect
                });

                return await fetch(modifiedRequest);
            })());
        } else {
            event.respondWith(fetch(event.request));
        }
    } catch (error) {
        console.error(error);   
    }
});
