// The purpose of this script is to pull any app specific index.html content into the
// BlazorUI project. This script is run by the index.html file in the WASMApp and MAUIApp 
// projects. This allows a single source of truth for both the WASMApp and MAUIApp projects.


links.forEach(function (linkInfo) {
    var link = document.createElement('link');
    Object.keys(linkInfo).forEach(function (key) {
        link.setAttribute(key, linkInfo[key]);
    });
    document.head.appendChild(link);
});

// Function to dynamically load a script
// Use this to load scripts for component libraries that would usually have you 
// place the script in the index.html file.
function loadScript(url) {
    var script = document.createElement('script');
    script.src = url;
    document.body.appendChild(script);
}

// Load Scripts



// Utility functions that are globally available to the app

// We need to wait for the DOM to be fully loaded before we can
// show the loading spinner. this is because the spinner depends
// on css that needs to fully load.
//document.addEventListener("DOMContentLoaded", function () {
//    document.getElementById('main').style.visibility = 'visible';
//});

// This function sends a message to the service worker to set the tenancy host url
window.setTenancyHostUrl = function (url) {
    if (navigator.serviceWorker.controller) {
        navigator.serviceWorker.controller.postMessage({
            type: 'SET_TENANCY_HOST_URL',
            url: url
        });
    }
}

// We intercept reloads so we can redirect to the init page instead of 
// staying on the current page.
window.onload = function () {
    console.log("Window loaded");
    if (performance.navigation.type === 1) { // 1 means the page is reloaded
        console.log("Page reload detected. Redirecting to root...");
        window.location.href = '/'; // Redirect to the root of the application
    }
}