using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Tests.Startup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Telegram.Tests.Unit;

public class RequestProcessorTests(UnitTestStartup setup) : IClassFixture<UnitTestStartup>
{
    private readonly ServiceProvider _services = setup.ServiceProvider;
    
}
