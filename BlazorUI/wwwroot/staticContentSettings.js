export const settings = {
    staticAssets: [
        // System (shared across all subtenants)
        { path: "system/base/System/", cacheType: "PreCache", shared: true },
        { path: "system/en-US/System/", cacheType: "PreCache", shared: true },
        { path: "system/es-MX/System/", cacheType: "LazyCache", shared: true },

        // StoreApp (shared across all subtenants)
        { path: "system/base/StoreApp/", cacheType: "PreCache", shared: true },
        { path: "system/en-US/StoreApp/", cacheType: "PreCache", shared: true },
        { path: "system/es-MX/StoreApp/", cacheType: "LazyCache", shared: true },

        // Tenancy (shared across all subtenants within tenant)
        { path: "tenancy/base/System/", cacheType: "PreCache", shared: true },
        { path: "tenancy/base/StoreApp/", cacheType: "PreCache", shared: true },
        { path: "tenancy/en-US/StoreApp/", cacheType: "PreCache", shared: true },
        { path: "tenancy/es-MX/StoreApp/", cacheType: "LazyCache", shared: true },

        // Subtenancy (subtenant-specific, requires cache swapping)
        { path: "subtenancy/base/System/", cacheType: "PreCache", shared: false },
        { path: "subtenancy/base/StoreApp/", cacheType: "PreCache", shared: false },
        { path: "subtenancy/en-US/StoreApp/", cacheType: "PreCache", shared: false },
        { path: "subtenancy/es-MX/StoreApp/", cacheType: "LazyCache", shared: false },

    ]
};