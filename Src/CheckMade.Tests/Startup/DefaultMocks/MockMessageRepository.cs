using CheckMade.Common.Interfaces;
using CheckMade.Common.Model;
using Moq;

namespace CheckMade.Tests.Startup.DefaultMocks;

internal class MockMessageRepository(IMock<IMessageRepository> mockMessageRepo) : IMessageRepository
{
    public async Task AddOrThrowAsync(InputMessageDto inputMessage)
    {
        await mockMessageRepo.Object.AddOrThrowAsync(inputMessage);
    }

    public async Task AddOrThrowAsync(IEnumerable<InputMessageDto> inputMessages)
    {
        await mockMessageRepo.Object.AddOrThrowAsync(inputMessages);
    }

    public async Task<IEnumerable<InputMessageDto>> GetAllOrThrowAsync()
    {
        return await mockMessageRepo.Object.GetAllOrThrowAsync();
    }

    public async Task<IEnumerable<InputMessageDto>> GetAllOrThrowAsync(TelegramUserId userId)
    {
        return await mockMessageRepo.Object.GetAllOrThrowAsync(userId);
    }

    public async Task HardDeleteAllOrThrowAsync(TelegramUserId userId)
    {
        await mockMessageRepo.Object.HardDeleteAllOrThrowAsync(userId);
    }
}