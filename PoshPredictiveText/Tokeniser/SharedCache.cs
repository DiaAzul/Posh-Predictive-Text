
namespace PoshPredictiveText
{
    using System.Collections.Concurrent;
    /// <summary>
    /// A simple cache to store processed items between scans of the command line.
    /// 
    /// The command line is parsed multiple times as each character is added (driven by
    /// the PSReadLine predictor. To reduce the amount of repeat processing that may be
    /// required, command line arguments which have already been processed are cached.
    /// 
    /// The cache is reset once the command line is exectuted so that cached items do
    /// not influence subsequent command lines entries.
    /// </summary>
    internal static class SharedCache
    {
        /// <summary>
        /// Dictionary of cached entries.
        /// </summary>
        private static readonly ConcurrentDictionary<string, CacheItem> cache = new();

        /// <summary>
        /// Add an item to the cache.
        /// </summary>
        /// <param name="key">Key to the cache item.</param>
        /// <param name="cacheItem">Cached item.</param>
        internal static void Add(string key, CacheItem cacheItem)
        {
            LOGGER.Write($"CACHE: Add key: {key}");
            cache.TryAdd(key, cacheItem);
        }

        /// <summary>
        /// Get an item from the cache.
        /// </summary>
        /// <param name="key">Key to the cache item.</param>
        /// <returns>Cached item. Null if no cached item.</returns>
        internal static CacheItem? Get(string key)
        {
            cache.TryGetValue(key, out CacheItem? cacheItem);
            if (cacheItem is not null){
                LOGGER.Write($"CACHE: Fetching key: {key}");
            }
            return cacheItem;
        }

        /// <summary>
        /// Reset the cache (remove all items).
        /// </summary>
        internal static void Reset()
        {
            LOGGER.Write("CACHE: Reset.");
            cache.Clear();
        }

        /// <summary>
        /// Count of items in the cache.
        /// </summary>
        internal static int Items
        {
            get { return cache.Count; }
        }
    }
}
