using Microsoft.Extensions.Caching.Memory;
using Polly.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollyPolicy.Repository.Service
{
    public class MemoryCacheProvider : IAsyncCacheProvider
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheProvider(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public async Task<(bool, object)> TryGetAsync(string key, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (_memoryCache.TryGetValue(key, out var cachedValue))
            {
                return (true, cachedValue);
            }

            return (false, null);
        }


        public async Task PutAsync(string key, object value, Ttl ttl, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl.Timespan
            };

            _memoryCache.Set(key, value, cacheEntryOptions);
        }

    }

}
