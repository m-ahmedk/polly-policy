using Microsoft.Extensions.Configuration;
using Polly;
using PollyPolicy.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollyPolicy.Repository.Service.Policies
{
    internal class FallbackPolicyService : IPolicy<IAsyncPolicy>
    {
        private int delay = 0;
        private readonly IConfiguration _configuration;

        public FallbackPolicyService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IAsyncPolicy> GetPolicy()
        {
            await assignValues();

            var fallbackPolicy = Policy
                .Handle<Exception>()
                .FallbackAsync((ct) => FallbackMethod(ct));

            return fallbackPolicy;
        }

        public async Task assignValues()
        {
            var delayValue = _configuration
                .GetSection("FallbackPolicyConfig:Delay")
                .Value;

            delay = string.IsNullOrEmpty(delayValue) ? 1000 : int.Parse(delayValue) * 1000;
        }

        static async Task FallbackMethod(CancellationToken cancellationToken)
        {
            Console.WriteLine("Fallback method called. Performing fallback action...");
            await Task.Delay(2000);
            Console.WriteLine("Fallback action completed.");
        }

    }
}