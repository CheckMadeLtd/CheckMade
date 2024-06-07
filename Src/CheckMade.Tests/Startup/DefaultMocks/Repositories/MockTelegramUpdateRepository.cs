using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model.Telegram.Updates;
using Moq;

namespace CheckMade.Tests.Startup.DefaultMocks.Repositories;

internal class MockTelegramUpdateRepository(IMock<ITelegramUpdateRepository> mockUpdateRepo) : ITelegramUpdateRepository
{
    public async Task AddAsync(TelegramUpdate telegramUpdate)
    {
        await mockUpdateRepo.Object.AddAsync(telegramUpdate);
    }

    public async Task AddAsync(IEnumerable<TelegramUpdate> telegramUpdates)
    {
        await mockUpdateRepo.Object.AddAsync(telegramUpdates);
    }

    public async Task<IEnumerable<TelegramUpdate>> GetAllAsync()
    {
        return await mockUpdateRepo.Object.GetAllAsync();
    }

    public async Task<IEnumerable<TelegramUpdate>> GetAllAsync(TelegramUserId userId)
    {
        return await mockUpdateRepo.Object.GetAllAsync(userId);
    }

    public async Task HardDeleteAllAsync(TelegramUserId userId)
    {
        await mockUpdateRepo.Object.HardDeleteAllAsync(userId);
    }
}