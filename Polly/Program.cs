using Polly;
using PollyPolicy.Repository.Factory;
using PollyPolicy.Enums;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using PollyPolicy.Repository.Interface;
using Microsoft.Extensions.DependencyInjection;
using Polly.Retry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Polly.CircuitBreaker;
using PollyPolicy.Repository.Service.Policies;
using Polly.Caching;
using Microsoft.Extensions.Caching.Memory;
using PollyPolicy.Repository.Service;

class Program
{
    private readonly IInstanceFactory<IPolicy<IAsyncPolicy>, PolicyType> _asyncPolicies;
    private readonly IInstanceFactory<IPolicy<AsyncCachePolicy<int>>, PolicyType> _asyncCachePolicies;

    public Program(
        IInstanceFactory<IPolicy<IAsyncPolicy>, PolicyType> asyncPolicies,
        IInstanceFactory<IPolicy<AsyncCachePolicy<int>>, PolicyType> asyncCachePolicies)
    {
        _asyncPolicies = asyncPolicies;
        _asyncCachePolicies = asyncCachePolicies;
    }

    static async Task Main(string[] args)
    {
        // Service provider to manage DI
        var serviceProvider = ConfigureServices();

        // Creating a scope and resolving the Program instance
        using (var scope = serviceProvider.CreateScope())
        {
            var program = scope.ServiceProvider.GetRequiredService<Program>();
            await program.RunAsync();
        }
    }

    // RunAsync method where the application logic resides
    private async Task RunAsync()
    {
        Console.WriteLine("Main started..");

        var getCircuitBreakerPolicy = await _asyncPolicies.GetInstance(PolicyType.CircuitBreakerPolicyType);
        var getFallbackPolicy = await _asyncPolicies.GetInstance(PolicyType.FallbackPolicyType);
        var getRetryPolicy = await _asyncPolicies.GetInstance(PolicyType.RetryPolicyType);
        var getTimeoutPolicy = await _asyncPolicies.GetInstance(PolicyType.TimeoutPolicyType);

        var getCachePolicy = await _asyncCachePolicies.GetInstance(PolicyType.CachePolicyType);

        var circuitbreakerPolicy = await getCircuitBreakerPolicy.GetPolicy();
        var fallbackPolicy = await getFallbackPolicy.GetPolicy();
        var retryPolicy = await getRetryPolicy.GetPolicy();
        var timeoutPolicy = await getTimeoutPolicy.GetPolicy();

        var cachePolicy = await getCachePolicy.GetPolicy();

        var wrapPolicy = Policy.WrapAsync(circuitbreakerPolicy, retryPolicy);

        try
        {
            while (true)
            {
                // Use the retry policy from IPolicyFactory to execute the AccessWebsite method.
                await wrapPolicy.ExecuteAsync(() => AccessWebsite());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }

    // ConfigureServices method to configure DI services.
    private static IServiceProvider ConfigureServices()
    {
        IConfiguration configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();

        var services = new ServiceCollection();

        // Adding to the DI container
        services.AddTransient<Program>();

        // classes that implements IPolicy
        services.AddScoped<IPolicy<AsyncCachePolicy>, CachePolicyService>();
        services.AddScoped<IPolicy<IAsyncPolicy>, CircuitBreakerPolicyService>();
        services.AddScoped<IPolicy<IAsyncPolicy>, FallbackPolicyService>();
        services.AddScoped<IPolicy<IAsyncPolicy>, RetryPolicyService>();
        services.AddScoped<IPolicy<IAsyncPolicy>, TimeoutPolicyService>();

        // Register PolicyFactory<IAsyncPolicy> for all policies using this
        services.AddScoped<IInstanceFactory<IPolicy<IAsyncPolicy>, PolicyType>, PolicyFactory<IAsyncPolicy>>();

        // Register PolicyFactory<AsyncCachePolicy> for all policies using this
        services.AddScoped<IInstanceFactory<IPolicy<AsyncCachePolicy>, PolicyType>, PolicyFactory<AsyncCachePolicy>>();

        services.AddSingleton<HttpClient>(); // needed as used in Program class
        services.AddSingleton<IConfiguration>(configuration); // needed as used in PolicyService classes
        services.AddSingleton<RemoteService>(); // Register RemoteService

        services.AddMemoryCache(); // IMemoryCache used

        return services.BuildServiceProvider(); // Build and return the service provider
    }

    static async Task AccessWebsite()
    {
        // Simulate an HTTP request
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync("https://www.seedlive.com");
        response.EnsureSuccessStatusCode();
    }

}