using Microsoft.Extensions.Configuration;
using Polly;
using PollyPolicy.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PollyPolicy.Repository.Service
{
    internal class CircuitBreakerPolicyService : IPolicy
    {
        private readonly IConfiguration _configuration;
        private int exceptionsAllowed = 0;
        private int breakDuration = 0;

        public CircuitBreakerPolicyService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IAsyncPolicy> GetPolicy()
        {
            await assignValues();

            var circuitBreakerPolicy = Policy
                .Handle<HttpRequestException>()
                .CircuitBreakerAsync(exceptionsAllowed, TimeSpan.FromSeconds(breakDuration),
                    onBreak: async (exception, timespan) =>
                    {
                        await Task.Delay(1000);
                        Console.WriteLine($"Circuit broken due to multiple failures.");
                    },
                    onReset: async () =>
                    {
                        await Task.Delay(1000);
                        Console.WriteLine("Circuit reset.");
                    });

            return circuitBreakerPolicy;
        }

        public async Task assignValues()
        {
            var exceptionAllowedValue = _configuration
                .GetRequiredSection("CircuitBreakerPolicy:ExceptionAllowed")
                .Value;

            var breakDurationValue = _configuration
                .GetRequiredSection("CircuitBreakerPolicy:BreakDuration")
                .Value;

            exceptionsAllowed = string.IsNullOrEmpty(exceptionAllowedValue) ? 2 : int.Parse(exceptionAllowedValue);
            breakDuration = string.IsNullOrEmpty(breakDurationValue) ? 20 : int.Parse(breakDurationValue);
        }
    }
}