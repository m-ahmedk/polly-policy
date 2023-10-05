using Polly.Retry;
using PollyPolicy.Enums;
using PollyPolicy.Repository.Interface;
using PollyPolicy.Repository.Service.Policies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollyPolicy.Repository.Factory
{
    internal class PolicyFactory<TPolicy> : IInstanceFactory<IPolicy<TPolicy>, PolicyType>
    {
        private readonly IEnumerable<IPolicy<TPolicy>> _policies;

        public PolicyFactory(IEnumerable<IPolicy<TPolicy>> policies)
        {
            _policies = policies;
        }

        public async Task<IPolicy<TPolicy>> GetInstance(PolicyType token)
        {
            return token switch
            {
                PolicyType.CircuitBreakerPolicyType => await GetService(typeof(CircuitBreakerPolicyService)),
                PolicyType.FallbackPolicyType => await GetService(typeof(FallbackPolicyService)),
                PolicyType.RetryPolicyType => await GetService(typeof(RetryPolicyService)),
                PolicyType.TimeoutPolicyType => await GetService(typeof(TimeoutPolicyService)),
                PolicyType.CachePolicyType => await GetService(typeof(CachePolicyService)),
                _ => throw new InvalidOperationException()
            };
        }

        public async Task<IPolicy<TPolicy>> GetService(Type type)
        {
            return _policies.FirstOrDefault(x => x.GetType() == type)!;
        }
    }
}