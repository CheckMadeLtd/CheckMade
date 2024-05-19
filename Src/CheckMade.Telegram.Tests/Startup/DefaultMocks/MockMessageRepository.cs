using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;
using Moq;

namespace CheckMade.Telegram.Tests.Startup.DefaultMocks;

internal class MockMessageRepository(IMock<IMessageRepository> mockMessageRepo) : IMessageRepository
{
    public async Task AddOrThrowAsync(InputMessage inputMessage)
    {
        await mockMessageRepo.Object.AddOrThrowAsync(inputMessage);
    }

    public async Task<IEnumerable<InputMessage>> GetAllOrThrowAsync()
    {
        return await mockMessageRepo.Object.GetAllOrThrowAsync();
    }

    public async Task<IEnumerable<InputMessage>> GetAllOrThrowAsync(long userId)
    {
        return await mockMessageRepo.Object.GetAllOrThrowAsync(userId);
    }

    public async Task HardDeleteAllOrThrowAsync(long userId)
    {
        await mockMessageRepo.Object.HardDeleteAllOrThrowAsync(userId);
    }
}