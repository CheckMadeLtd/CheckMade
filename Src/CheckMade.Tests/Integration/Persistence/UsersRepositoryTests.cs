using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Integration.Persistence;

public class UsersRepositoryTests
{
    private IServiceProvider? _services;

    [Fact]
    public async Task GetAsync_ReturnsSpecificTestUser_IncludingCurrentVendor()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();

        var repo = _services.GetRequiredService<IUsersRepository>();
        var user = await repo.GetAsync(LukasDe);

        Assert.NotNull(user);
        Assert.True(user.Equals(LukasDe));
        Assert.Equal(
            EveConGmbH,
            user.CurrentEmployer.GetValueOrThrow());
    }
    
    [Fact]
    public async Task GetAsync_ReturnsSpecificTestUser_WithNoCurrentVendor()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();

        var repo = _services.GetRequiredService<IUsersRepository>();
        var user = await repo.GetAsync(DanielEn);

        Assert.NotNull(user);
        Assert.True(user.Equals(DanielEn));
        Assert.True(user.CurrentEmployer.IsNone);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsIntegrationTestUsers_WithCorrectOneToManyRolesMapping()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        
        var usersRepo = _services.GetRequiredService<IUsersRepository>();
        
        var users = 
            (await usersRepo.GetAllAsync())
            .ToList();
        
        Assert.Equal(
            DanielEn.FirstName,
            users[0].FirstName);
        Assert.Equal(
            LukasDe.FirstName,
            users[1].FirstName);
        Assert.Equal(
            SaniCleanAdmin_DanielEn_X2024.Token,
            users[0].HasRoles.First().Token);
        Assert.Equal(
            LiveEventAdmin_DanielEn_X2024.Token,
            users[0].HasRoles.Last().Token);
    }
}