// The purpose of this script is to pull any app specific index.html content into the
// BlazorUI project. This script is run by the index.html file in the WASMApp and MAUIApp
// projects. This allows a single source of truth for both the WASMApp and MAUIApp projects.

var basebodyscript = document.createElement('script');
basebodyscript.src = '_content/BaseApp.BlazorUI/baseindexbody.js';
document.body.appendChild(basebodyscript);


var bodylinks = [
    // example:
    //    { href: '_content/BlazorUI/myimage.png', rel: 'icon', type: 'image/png' },
];

bodylinks.forEach(function (linkInfo) {
    var link = document.createElement('link');
    Object.keys(linkInfo).forEach(function (key) {
        link.setAttribute(key, linkInfo[key]);
    });
    document.head.appendChild(link);
});

window.getClientFingerprint = function () {
    console.log("getClientFingerprint() called");
    const client = new ClientJS();
    return client.getFingerprint();
};

// Function to dynamically load a script
// Use this to load scripts for component libraries that would usually have you 
// place the script in the index.html file.
function loadScript(url) {
    var script = document.createElement('script');
    script.src = url;
    document.body.appendChild(script);
}

// Load Scripts


// This function sends a message to the service worker to set the tenancy host url
window.setAssetHostUrl = function (url) {
    window.assetHostUrl = new URL(url);
    if (navigator.serviceWorker.controller) {
        navigator.serviceWorker.controller.postMessage({
            type: 'SET_ASSET_HOST_URL',
            url: url
        });
    }
}