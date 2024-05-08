using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;
using Moq;

namespace CheckMade.Telegram.Tests.Startup.DefaultMocks;

internal class MockMessageRepo(IMock<IMessageRepo> mockMessageRepo) : IMessageRepo
{
    public async Task AddAsync(InputTextMessage inputMessage)
    {
        await mockMessageRepo.Object.AddAsync(inputMessage);
    }

    public async Task<IEnumerable<InputTextMessage>> GetAllAsync(long userId)
    {
        return await mockMessageRepo.Object.GetAllAsync(userId);
    }
}