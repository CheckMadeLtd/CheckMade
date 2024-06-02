using CheckMade.Common.Interfaces;
using CheckMade.Common.Model.TelegramUpdates;
using Moq;

namespace CheckMade.Tests.Startup.DefaultMocks;

internal class MockMessageRepository(IMock<IMessageRepository> mockMessageRepo) : IMessageRepository
{
    public async Task AddOrThrowAsync(TelegramUpdateDto telegramUpdate)
    {
        await mockMessageRepo.Object.AddOrThrowAsync(telegramUpdate);
    }

    public async Task AddOrThrowAsync(IEnumerable<TelegramUpdateDto> inputMessages)
    {
        await mockMessageRepo.Object.AddOrThrowAsync(inputMessages);
    }

    public async Task<IEnumerable<TelegramUpdateDto>> GetAllOrThrowAsync()
    {
        return await mockMessageRepo.Object.GetAllOrThrowAsync();
    }

    public async Task<IEnumerable<TelegramUpdateDto>> GetAllOrThrowAsync(TelegramUserId userId)
    {
        return await mockMessageRepo.Object.GetAllOrThrowAsync(userId);
    }

    public async Task HardDeleteAllOrThrowAsync(TelegramUserId userId)
    {
        await mockMessageRepo.Object.HardDeleteAllOrThrowAsync(userId);
    }
}