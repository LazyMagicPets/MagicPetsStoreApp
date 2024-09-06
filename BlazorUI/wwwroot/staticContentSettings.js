export default {
    staticAssets: [
        // System
        { "/Assets/base/System/": "PreCache" },
        { "/Assets/en-US/System/": "PreCache" },
        { "/Assets/es-MX/System/": "LazyCache" },

        // StoreApp
        { "Assets/base/StoreApp/": "PreCache" },
        { "/Assets/en-US/StoreApp/": "PreCache" },
        { "/Assets/es-MX/StoreApp/": "LazyCache" },

        // Tenancy
        { "/Tenancy/base/StoreApp/": "PreCache" },
        { "/Tenancy/en-US/StoreApp/": "PreCache" },
        { "/Tenancy/es-MX/StoreApp/": "LazyCache" },
    ]
};