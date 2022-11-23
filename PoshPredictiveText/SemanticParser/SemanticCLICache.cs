
namespace PoshPredictiveText.SemanticParser
{
    using System.Collections.Concurrent;
    /// <summary>
    /// Caches semanticCLIs to reduce the number of times that the command line
    /// needs parsing. Assumption is that the Predictor will perform have
    /// parsed the command line prior to tab-expansion call to parse exactly
    /// the same command line.
    /// 
    /// During testing multiple predictors are created and tested. These
    /// will store values in the cache. Each predictor created during testing 
    /// has a unique guid. This is used to store the generated tokeniser for 
    /// that specific test in the cache. Tab-expansion will only use the cached
    /// tokeniser when it matches the expected guid for the production predictor.
    /// </summary>
    internal class SemanticCLICache : IDisposable
    {
        private static readonly ConcurrentDictionary<string, SemanticCLI> cache = new();
        private static readonly Mutex available = new();

        internal bool Acquired { get; set; } = false;

        /// <summary>
        /// Acquire unique access to the cache.
        /// </summary>
        internal SemanticCLICache()
        {
            if (available.WaitOne(30))
            {
                Acquired = true;
            }
            else
            {
                Acquired = false;
            }
        }

        /// <summary>
        /// Stash a new tokeniser in the cache for a given guid.
        /// </summary>
        /// <param name="newTokeniser">SemanticCLI to stash.</param>
        /// <param name="guid">Guid of the application stashing the tokeniser.</param>
        internal static void Stash(string key, SemanticCLI semanticCLI)
        {
            LOGGER.Write($"MS CACHE: Caching {key}. {semanticCLI.Count} tokens in state machine.");
            if (cache.TryGetValue(key, out SemanticCLI? _))
            {
                LOGGER.Write($"MS CACHE: {key} Already exists.");
            }
            _ = cache.AddOrUpdate(key, semanticCLI, (key, oldValue) => semanticCLI);
        }

        /// <summary>
        /// Get the tokeniser associated with a guid from the cache.
        /// </summary>
        /// <param name="guid">guid of tokeniser to get.</param>
        /// <returns>SemanticCLI</returns>
        internal static SemanticCLI? Get(string key)
        {
            if (cache.TryGetValue(key, out SemanticCLI? semanticCLI))
            {
                LOGGER.Write($"MS CACHE: Retrieved {key}. {semanticCLI.Count} tokens in state machine.");
            }
            else
            {
                LOGGER.Write($"MS CACHE: Cache miss for {key}.");
            }
            return semanticCLI;
        }

        /// <summary>
        /// Clear the cache of all entries.
        /// </summary>
        internal static void Remove(string key)
        {
            _ = cache.TryRemove(key, out SemanticCLI _);
        }

        internal static void Clear()
        {
            cache.Clear();
        }

        /// <summary>
        /// Release the cache for access once processing complete.
        /// </summary>
        void IDisposable.Dispose()
        {
            available.ReleaseMutex();
        }
    }
}
