using CheckMade.Common.Persistence;
using CheckMade.Telegram.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../"));

var configBuilder = new ConfigurationBuilder()
    .SetBasePath(projectRoot)
    .AddJsonFile("local.settings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

var config = configBuilder.Build();

var services = new ServiceCollection();

services.AddLogging(loggingConfig =>
{
    loggingConfig.ClearProviders();
    loggingConfig.AddConsole(); 
    loggingConfig.AddDebug(); 
});

// var dbConnString = 

// services.Add_CommonPersistence_Dependencies();
services.Add_TelegramPersistence_Dependencies();


using (var serviceProvider = services.BuildServiceProvider())
{
    // Here, use GetRequiredService<IMyService>()
    // myService.DoSomething()
    // downstream dependencies will be resolved automatically if registered above. 
}
