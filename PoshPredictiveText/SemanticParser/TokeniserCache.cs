
namespace PoshPredictiveText.SemanticParser
{
    using System.Collections.Concurrent;
    /// <summary>
    /// Caches tokenisers to reduce the number of times that the command line
    /// needs parsing. Assumption is that the Predictor will perform have
    /// parsed the command line prior to tab-expansion call to parse exactly
    /// the same command line.
    /// 
    /// Note, during testing multiple predictors are created and tested. These
    /// will store values in the cache. Each predictor created during testing 
    /// has a unique guid. This is used to store the generated tokeniser for 
    /// that specific test in the cache. Tab-expansion will only use the cached
    /// tokeniser when it matches the expected guid for the production predictor.
    /// </summary>
    internal class TokeniserCache : IDisposable
    {
        private static readonly ConcurrentDictionary<string, Tokeniser> tokenisers = new();
        private static readonly Mutex available = new();

        internal bool Acquired { get; set; } = false;

        /// <summary>
        /// Acquire unique access to the cache.
        /// </summary>
        internal TokeniserCache()
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
        /// <param name="newTokeniser">Tokeniser to stash.</param>
        /// <param name="guid">Guid of the application stashing the tokeniser.</param>
        internal static void Stash(Tokeniser tokeniser, string key)
        {
            tokenisers[key] = tokeniser;
        }

        /// <summary>
        /// Get the tokeniser associated with a guid from the cache.
        /// </summary>
        /// <param name="guid">guid of tokeniser to get.</param>
        /// <returns>Tokeniser</returns>
        internal static Tokeniser? Get(string key)
        {
            _ = tokenisers.TryGetValue(key, out var tokeniser);
            return tokeniser;
        }

        /// <summary>
        /// Clear the cache of all entries.
        /// </summary>
        internal static void Remove(string key)
        {
            _ = tokenisers.TryRemove(key, out Tokeniser _);
        }

        internal static void Clear()
        {
            tokenisers.Clear();
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
