using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Telegram.Input;
using Moq;

namespace CheckMade.Tests.Startup.DefaultMocks.Repositories.Tlg;

internal class MockTlgInputRepository(IMock<ITlgInputRepository> mockInputRepo) : ITlgInputRepository
{
    public async Task AddAsync(TlgInput tlgInput)
    {
        await mockInputRepo.Object.AddAsync(tlgInput);
    }

    public async Task AddAsync(IEnumerable<TlgInput> tlgInputs)
    {
        await mockInputRepo.Object.AddAsync(tlgInputs);
    }

    public async Task<IEnumerable<TlgInput>> GetAllAsync()
    {
        return await mockInputRepo.Object.GetAllAsync();
    }

    public async Task<IEnumerable<TlgInput>> GetAllAsync(TlgUserId userId)
    {
        return await mockInputRepo.Object.GetAllAsync(userId);
    }

    public async Task HardDeleteAllAsync(TlgUserId userId)
    {
        await mockInputRepo.Object.HardDeleteAllAsync(userId);
    }
}