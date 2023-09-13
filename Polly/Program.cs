using Polly;
using PollyPolicy.Repository.Factory;
using PollyPolicy.Repository.Service;
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

class Program
{
    private readonly PolicyFactory _policyFactory;
    private HttpClient _httpClient;

    public Program(PolicyFactory policyFactory, HttpClient httpClient)
    {
        _httpClient = httpClient;
        _policyFactory = policyFactory;
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

        var getCircuitBreakerPolicy = await _policyFactory.GetInstance(PolicyType.CircuitBreakerPolicyType);
        var getFallbackPolicy = await _policyFactory.GetInstance(PolicyType.FallbackPolicyType);
        var getRetryPolicy = await _policyFactory.GetInstance(PolicyType.RetryPolicyType);
        var getTimeoutPolicy = await _policyFactory.GetInstance(PolicyType.TimeoutPolicyType);

        var circuitbreakerPolicy = await getCircuitBreakerPolicy.GetPolicy();
        var fallbackPolicy = await getFallbackPolicy.GetPolicy();
        var retryPolicy = await getRetryPolicy.GetPolicy();
        var timeoutPolicy = await getTimeoutPolicy.GetPolicy();

        var wrapPolicy = Policy.WrapAsync(circuitbreakerPolicy, fallbackPolicy);

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
        services.AddScoped<IPolicy, CircuitBreakerPolicyService>();
        services.AddScoped<IPolicy, FallbackPolicyService>();
        services.AddScoped<IPolicy, RetryPolicyService>();
        services.AddScoped<IPolicy, TimeoutPolicyService>();

        services.AddSingleton<IInstanceFactory<IPolicy, PolicyType>, PolicyFactory>();

        services.AddSingleton<PolicyFactory>(); // needed as used in Program class
        services.AddSingleton<HttpClient>(); // needed as used in Program class
        services.AddSingleton<IConfiguration>(configuration); // needed as used in PolicyService classes

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