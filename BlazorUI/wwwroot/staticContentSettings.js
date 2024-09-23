export default {
    staticAssets: [
        // System
        { "/System/base/System/": "PreCache" },
        { "/System/en-US/System/": "PreCache" },
        { "/System/es-MX/System/": "LazyCache" },

        // StoreApp
        { "/System/base/StoreApp/": "PreCache" },
        { "/System/en-US/StoreApp/": "PreCache" },
        { "/System/es-MX/StoreApp/": "LazyCache" },

        // Tenancy
        { "/Tenancy/base/StoreApp/": "PreCache" },
        { "/Tenancy/en-US/StoreApp/": "PreCache" },
        { "/Tenancy/es-MX/StoreApp/": "LazyCache" },
    ]
};