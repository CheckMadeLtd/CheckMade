using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using Moq;

namespace CheckMade.Tests.Startup.DefaultMocks.Repositories.ChatBot;

internal class MockTlgInputsRepository(IMock<ITlgInputsRepository> mockInputRepo) : ITlgInputsRepository
{
    public async Task AddAsync(TlgInput tlgInput)
    {
        await mockInputRepo.Object.AddAsync(tlgInput);
    }

    public async Task AddAsync(IReadOnlyCollection<TlgInput> tlgInputs)
    {
        await mockInputRepo.Object.AddAsync(tlgInputs);
    }

    public async Task<IEnumerable<TlgInput>> GetAllAsync(TlgUserId userId)
    {
        return await mockInputRepo.Object.GetAllAsync(userId);
    }

    public async Task<IEnumerable<TlgInput>> GetAllAsync(TlgAgent tlgAgent)
    {
        return await mockInputRepo.Object.GetAllAsync(tlgAgent);
    }

    public async Task HardDeleteAllAsync(TlgAgent tlgAgent)
    {
        await mockInputRepo.Object.HardDeleteAllAsync(tlgAgent);
    }
}