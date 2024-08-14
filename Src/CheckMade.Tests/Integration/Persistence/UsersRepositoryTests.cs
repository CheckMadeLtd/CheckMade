using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Integration.Persistence;

public sealed class UsersRepositoryTests
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

        var danielEn = users.First(u => u.Mobile.Equals(DanielEn.Mobile));
        var lukasDe = users.First(u => u.Mobile.Equals(LukasDe.Mobile));
        
        Assert.Equal(DanielEn.FirstName, danielEn.FirstName);
        Assert.Equal(LukasDe.FirstName, lukasDe.FirstName);
        
        Assert.Contains(
            SanitaryAdmin_DanielEn_X2024.Token,
            danielEn.HasRoles.Select(r => r.Token));
        Assert.Contains(
            SanitaryEngineer_DanielEn_Y2024.Token,
            danielEn.HasRoles.Select(r => r.Token));
    }
}