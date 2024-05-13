using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register needed dependencies here

using (var serviceProvider = services.BuildServiceProvider())
{
    // Here, use GetRequiredService<IMyService>()
    // myService.DoSomething()
    // downstream dependencies will be resolved automatically if registered above. 
}
