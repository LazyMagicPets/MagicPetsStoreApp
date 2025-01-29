export default {
    staticAssets: [
        // System
        { "system/base/System/": "PreCache" },
        { "system/en-US/System/": "PreCache" },
        { "system/es-MX/System/": "LazyCache" },

        // StoreApp
        { "system/base/StoreApp/": "PreCache" },
        { "system/en-US/StoreApp/": "PreCache" },
        { "system/es-MX/StoreApp/": "LazyCache" },

        // Tenancy
        { "tenancy/base/System/": "PreCache" },
        { "tenancy/base/StoreApp/": "PreCache" },
        { "tenancy/en-US/StoreApp/": "PreCache" },
        { "tenancy/es-MX/StoreApp/": "LazyCache" },

        // Subtenancy
        { "subtenancy/base/System/": "PreCache" },
        { "subtenancy/base/StoreApp/": "PreCache" },
        { "subtenancy/en-US/StoreApp/": "PreCache" },
        { "subtenancy/es-MX/StoreApp/": "LazyCache" },

    ]
};