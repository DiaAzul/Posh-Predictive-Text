
namespace PoshPredictiveText.SemanticParser
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
    /// 
    internal static class MachineStateCache
    {
        /// <summary>
        /// Dictionary of cached entries.
        /// </summary>
        private static readonly ConcurrentDictionary<string, MachineState> cache = new();

        /// <summary>
        /// Add an item to the cache.
        /// </summary>
        /// <param name="key">Key to the cache item.</param>
        /// <param name="stateMachineState">Cached item.</param>
        internal static void Add(string key, MachineState cachedMachineState)
        {
            if (cache.TryAdd(key, cachedMachineState))
            {
                LOGGER.Write($"CACHE: Added value for key: {key}");
                return;
            }
            if (cache.TryRemove(key,out _))
            {
                if (cache.TryAdd(key, cachedMachineState))
                {
                    LOGGER.Write($"CACHE: Updated value for key: {key}");
                    return;
                }
            }
        }

        /// <summary>
        /// Get an item from the cache.
        /// </summary>
        /// <param name="key">Key to the cache item.</param>
        /// <returns>Cached item. Null if no cached item.</returns>
        internal static MachineState? Get(string key)
        {
            if (cache.TryGetValue(key, out var stateMachineState))
            {
                LOGGER.Write($"CACHE: Got value for key: {key}");
                return stateMachineState;
            }
            return null;

        }

        internal static bool TryGetValue(string key, out MachineState cachedMachineState)
        {
            if (cache.TryGetValue(key, out MachineState? stateMachineState))
            {
                if (stateMachineState is not null)
                {
                    cachedMachineState = stateMachineState;
                    return true;
                }
            };
            cachedMachineState = new MachineState();
            return false;
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
