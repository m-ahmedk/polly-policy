using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Retry;
using PollyPolicy.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollyPolicy.Repository.Service
{
    internal class RetryPolicyService : IPolicy
    {
        private readonly IConfiguration _configuration;
        private int maxRetries = 0;
        private int delay = 0;

        public RetryPolicyService(IConfiguration configuration) {
            _configuration = configuration;
        }
        

        public async Task<IAsyncPolicy> GetPolicy()
        {
            await assignValues();

            var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .WaitAndRetryAsync(maxRetries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) / 2),
                onRetry: async (exception, retryCount) =>
                {
                    Console.WriteLine($"Retrying... Attempt {retryCount}");
                    await Task.Delay(delay);
                });

            return retryPolicy;
        }

        public async Task assignValues()
        {
            var maxRetriesValue = _configuration
            .GetSection("RetryPolicyConfig:MaxRetries")
            .Value;

            var delayValue = _configuration
            .GetSection("RetryPolicyConfig:Delay")
            .Value;

            maxRetries = string.IsNullOrEmpty(maxRetriesValue) ? 1 : int.Parse(maxRetriesValue);
            delay = string.IsNullOrEmpty(delayValue) ? 1000 : int.Parse(delayValue) * 1000;
        }
    }
}