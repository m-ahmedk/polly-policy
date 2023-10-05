using System;
using Microsoft.Extensions.Caching.Memory;

public interface IMemoryCacheProvider
{
    bool TryGetValue<TItem>(string key, out TItem value);
    void Set<TItem>(string key, TItem value, TimeSpan absoluteExpirationRelativeToNow);
}