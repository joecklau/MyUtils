using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.AspNetCore.Mvc.TagHelpers.Internal;
using Microsoft.Extensions.Caching.Memory;

public static class CacheTagUtils
{
    /// <summary>
    /// Get <see cref="CacheTagKey"/> for <see cref="CacheTagHelper"/>s by <paramref name="varyBys"/>
    /// </summary>
    /// <param name="memoryCache"></param>
    /// <param name="varyBys"></param>
    /// <returns></returns>
    public static IEnumerable<CacheTagKey> GetCacheTagKeys(this IMemoryCache memoryCache, params string[] varyBys) =>
        memoryCache.GetKeys<CacheTagKey>().Where(key => typeof(CacheTagKey).GetField("_varyBy", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(key) is string varyBy && varyBys.Contains(varyBy));

    /// <summary>
    /// <inheritdoc cref="GetCacheTagKeys(IMemoryCache, string[])"/>
    /// </summary>
    /// <param name="cacheFactory"></param>
    /// <param name="varyBys"></param>
    /// <returns></returns>
    public static IEnumerable<CacheTagKey> GetCacheTagKeys(this CacheTagHelperMemoryCacheFactory cacheFactory, params string[] varyBys) => cacheFactory.Cache.GetCacheTagKeys(varyBys);

    /// <summary>
    /// Clear <see cref="CacheTagHelper"/>s by <paramref name="varyBys"/>
    /// </summary>
    /// <param name="memoryCache"></param>
    /// <param name="varyBys"></param>
    public static void ClearCacheTags(this IMemoryCache memoryCache, params string[] varyBys) =>
        memoryCache.GetCacheTagKeys(varyBys).ToList().ForEach(memoryCache.Remove);

    /// <summary>
    /// <inheritdoc cref="ClearCacheTags(IMemoryCache, string[])"/>
    /// </summary>
    /// <param name="cacheFactory"></param>
    /// <param name="varyBys"></param>
    public static void ClearCacheTags(this CacheTagHelperMemoryCacheFactory cacheFactory, params string[] varyBys) => cacheFactory.Cache.ClearCacheTags(varyBys);

}
