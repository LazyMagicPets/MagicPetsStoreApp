// The purpose of this script is to pull any app specific index.html content into the
// BlazorUI project. This script is run by the index.html file in the WASMApp and MAUIApp 
// projects. This allows a single source of truth for both the WASMApp and MAUIApp projects.

document.title = "Store App";
var metaCharset = document.createElement('meta');
metaCharset.setAttribute('charset', 'utf-8');
document.head.appendChild(metaCharset);

var baseheadscript = document.createElement('script');
baseheadscript.src = '_content/BaseApp.BlazorUI/baseindexhead.js';
document.head.appendChild(baseheadscript);

var links = [
];

links.forEach(function (linkInfo) {
    var link = document.createElement('link');
    Object.keys(linkInfo).forEach(function (key) {
        link.setAttribute(key, linkInfo[key]);
    });
    document.head.appendChild(link);
});
