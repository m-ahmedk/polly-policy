using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollyPolicy.Repository.Interface
{
    internal interface IInstanceFactory<T, U>
    {
        public Task<T> GetInstance(U token);
    }
}
