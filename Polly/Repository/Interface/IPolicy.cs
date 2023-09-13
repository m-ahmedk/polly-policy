using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollyPolicy.Repository.Interface
{
    internal interface IPolicy
    {
        public Task<IAsyncPolicy> GetPolicy();
        public Task assignValues();
    }
}
