using System.Data.Common;
using CheckMade.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

namespace CheckMade.Common.Persistence;

public static class DependencyRegistration
{
    public static void Add_CommonPersistence_Dependencies(this IServiceCollection services, string dbConnectionString)
    {
        var dbRetryPolicy = Policy
            .Handle<DbException>()
            .WaitAndRetryAsync(3, 
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
                (exception, timeSpan, retryCount) =>
                {
                    // This will be ignored by xUnit (who only works with ITestOutputHelper) but should work for prd.
                    Console.Error.WriteLine($"Database error occurred at attempt {retryCount} with delay of " +
                                            $"{timeSpan.TotalMilliseconds} milliseconds!" +
                                            $" Exception message: {exception.Message}");
                });
        
        services.AddSingleton(dbRetryPolicy);

        services.AddScoped<IDbConnectionProvider>(_ => new DbConnectionProvider(dbConnectionString));
        services.AddScoped<IDbExecutionHelper>(sp =>
            new DbExecutionHelper(sp.GetRequiredService<IDbConnectionProvider>(),
                sp.GetRequiredService<AsyncRetryPolicy>()));
    }
}
