using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Tlg.Input;
using Moq;

namespace CheckMade.Tests.Startup.DefaultMocks.Repositories.Tlg;

internal class MockTlgUpdateRepository(IMock<ITlgUpdateRepository> mockUpdateRepo) : ITlgUpdateRepository
{
    public async Task AddAsync(TlgUpdate tlgUpdate)
    {
        await mockUpdateRepo.Object.AddAsync(tlgUpdate);
    }

    public async Task AddAsync(IEnumerable<TlgUpdate> tlgUpdates)
    {
        await mockUpdateRepo.Object.AddAsync(tlgUpdates);
    }

    public async Task<IEnumerable<TlgUpdate>> GetAllAsync()
    {
        return await mockUpdateRepo.Object.GetAllAsync();
    }

    public async Task<IEnumerable<TlgUpdate>> GetAllAsync(TlgUserId userId)
    {
        return await mockUpdateRepo.Object.GetAllAsync(userId);
    }

    public async Task HardDeleteAllAsync(TlgUserId userId)
    {
        await mockUpdateRepo.Object.HardDeleteAllAsync(userId);
    }
}