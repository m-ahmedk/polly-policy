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
    internal class TimeoutPolicyService : IPolicy<IAsyncPolicy>
    {
        private readonly IConfiguration _configuration;
        private int timeOutSecond = 0;

        public TimeoutPolicyService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IAsyncPolicy> GetPolicy()
        {
            await assignValues();

            var timeoutPolicy = Policy
                .TimeoutAsync(TimeSpan.FromSeconds(timeOutSecond));

            return timeoutPolicy;
        }

        public async Task assignValues()
        {
            var timeOutValue = _configuration
            .GetSection("TimeoutPolicyConfig:TimeOutSecond")
            .Value;

            timeOutSecond = string.IsNullOrEmpty(timeOutValue) ? 1 : int.Parse(timeOutValue);
        }

    }
}
