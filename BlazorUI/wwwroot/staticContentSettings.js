export default {
    staticAssets: [
        { "/Assets/base/System/": "PreCache" },
        { "/Assets/en-US/System/": "PreCache" },
        { "/Assets/es-MX/System/": "LazyCache" },

        { "Assets/base/StoreApp/": "PreCache" },
        { "/Assets/en-US/StoreApp/": "PreCache" },
        { "/Assets/es-MX/StoreApp/": "LazyCache" },

        { "/Tenancy/base/StoreApp/": "PreCache" },
        { "/Tenancy/en-US/StoreApp/": "PreCache" },
        { "/Tenancy/es-MX/StoreApp/": "LazyCache" },
    ]
};