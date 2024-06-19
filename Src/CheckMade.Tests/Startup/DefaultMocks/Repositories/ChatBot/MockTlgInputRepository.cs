using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using Moq;

namespace CheckMade.Tests.Startup.DefaultMocks.Repositories.ChatBot;

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

    public async Task<IEnumerable<TlgInput>> GetAllAsync(TlgUserId userId, TlgChatId chatId)
    {
        return await mockInputRepo.Object.GetAllAsync(userId, chatId);
    }

    public async Task HardDeleteAllAsync(TlgUserId userId)
    {
        await mockInputRepo.Object.HardDeleteAllAsync(userId);
    }
}