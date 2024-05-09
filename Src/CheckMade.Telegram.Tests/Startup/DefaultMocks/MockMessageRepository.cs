using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;
using Moq;

namespace CheckMade.Telegram.Tests.Startup.DefaultMocks;

internal class MockMessageRepository(IMock<IMessageRepository> mockMessageRepo) : IMessageRepository
{
    public async Task AddAsync(InputTextMessage inputMessage)
    {
        await mockMessageRepo.Object.AddAsync(inputMessage);
    }

    public async Task<IEnumerable<InputTextMessage>> GetAllAsync(long userId)
    {
        return await mockMessageRepo.Object.GetAllAsync(userId);
    }

    public async Task HardDeleteAsync(long userId)
    {
        await mockMessageRepo.Object.HardDeleteAsync(userId);
    }
}