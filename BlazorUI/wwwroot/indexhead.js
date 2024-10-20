// The purpose of this script is to pull any app specific index.html content into the
// BlazorUI project. This script is run by the index.html file in the WASMApp and MAUIApp 
// projects. This allows a single source of truth for both the WASMApp and MAUIApp projects.

document.title = "Store App";
var metaCharset = document.createElement('meta');
metaCharset.setAttribute('charset', 'utf-8');
document.head.appendChild(metaCharset);

// Link tags
var links = [
    { href: '/system/base/System/favicon.png', rel: 'icon', type: 'image/png' },
    { href: 'https://fonts.googleapis.com/css2?family=Roboto:wght@300;400;500;700&display=swap', rel: 'stylesheet' },
    { href: '_content/MudBlazor/MudBlazor.min.css', rel: 'stylesheet' }
];

links.forEach(function (linkInfo) {
    var link = document.createElement('link');
    Object.keys(linkInfo).forEach(function (key) {
        link.setAttribute(key, linkInfo[key]);
    });
    document.head.appendChild(link);
});
