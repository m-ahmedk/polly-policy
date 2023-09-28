using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Caching;
using PollyPolicy.Repository.Interface;

namespace PollyPolicy.Repository.Service.Policies
{
    internal class CachePolicyService : IPolicy<AsyncCachePolicy>
    {
        private readonly IConfiguration _configuration;
        //private readonly IMemoryCacheProvider _cacheProvider;
        private readonly RemoteService _remoteService;

        // Create a MemoryCache instance
        private readonly IMemoryCache _memoryCache;

        private string apiUrl;
        private int timeOutSecond = 0;

        public CachePolicyService(IConfiguration configuration,
           // IMemoryCacheProvider memoryCacheProvider,
            RemoteService remoteService,
            IMemoryCache memoryCache)
        {
            _configuration = configuration;
            //_cacheProvider = memoryCacheProvider;
            _remoteService = remoteService;
            _memoryCache = memoryCache;
        }

        public async Task<AsyncCachePolicy> GetPolicy()
        {
            await assignValues();

            // Define a memory cache provider
            var memoryCacheProvider = new MemoryCacheProvider(_memoryCache);

            // Define a CachePolicy with a 10-second expiration time
            var cachePolicy = Policy
            .CacheAsync(memoryCacheProvider, TimeSpan.FromSeconds(10), (context, key, ex) =>
            {
                Console.WriteLine($"Cache hit for key: {key}");
            });

            return cachePolicy;

        }

        public async Task assignValues()
        {
            var apiUrlValue = _configuration
            .GetSection("CachePolicyConfig:Url")
            .Value; 
            
            var timeOutValue = _configuration
            .GetSection("CachePolicyConfig:TimeOutSecond")
            .Value;

            apiUrl = apiUrlValue ?? "https://example.com";
            timeOutSecond = string.IsNullOrEmpty(timeOutValue) ? 10 : int.Parse(timeOutValue);
        }

    }
}
