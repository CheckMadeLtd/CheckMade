using CheckMade.Common.Model;

namespace CheckMade.Common.Interfaces;

public interface IMessageRepository
{
    Task AddOrThrowAsync(InputMessageDto inputMessage);
    Task AddOrThrowAsync(IEnumerable<InputMessageDto> inputMessages);
    Task<IEnumerable<InputMessageDto>> GetAllOrThrowAsync();
    Task<IEnumerable<InputMessageDto>> GetAllOrThrowAsync(TelegramUserId userId);
    Task HardDeleteAllOrThrowAsync(TelegramUserId userId);
}