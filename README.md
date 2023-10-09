# Overview
This project is a demonstration of how Polly policies can be implemented in a .NET Core Command Line Interface (CLI) application. Polly is a resilience and transient-fault-handling library that helps developers handle faults and failures in a flexible and customizable way.

# Project Structure
The project consists of the following components:

- PolicyFactory: This component is responsible for producing different Polly policies based on the requirements. These policies implement a common interface, allowing users to interact with them in a consistent manner.

- Interfaces: All policies implemented by the PolicyFactory adhere to the same interface. This design choice enables users to call functions on different policies using a unified approach.

- CLI Application: The main application is a .NET Core CLI that showcases the behavior of different policies when a specified website is not accessible. The URL for the website is defined in the appsettings of the project.

- Enums: There are five policies implemented on this application. They are set as enums which are **CachePolicyType**, **CircuitBreakerPolicyType**, **FallbackPolicyType**, **RetryPolicyType** and **TimeoutPolicyType**.
        
# How to Use

1. Clone the Repository:

``` 
git clone https://github.com/m-ahmedk/resilient-web-cli.git
cd your-repo
```

2. Configure Website URL: <br>
Update the website URL in the appsettings.json file.

3. Build & Run:

```
dotnet build
dotnet run
```

## Example Usage
```
// Assign type of Policy, in this case RetryPolicy
var getRetryPolicy = await _asyncPolicies.GetInstance(PolicyType.RetryPolicyType);

// Get the policy's configured settings
var retryPolicy = await getRetryPolicy.GetPolicy();

// Execute a function using the policy
retryPolicy.Execute(() =>
{
    // Your code that may encounter transient faults
});

```

## Contact
For questions or further assistance, please contact me at m.ahmedk287@gmail.com

Enjoy exploring the power of Polly policies in your .NET Core applications!