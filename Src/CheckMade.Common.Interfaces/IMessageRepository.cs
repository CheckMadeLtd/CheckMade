using CheckMade.Common.Model.TelegramUpdates;

namespace CheckMade.Common.Interfaces;

public interface IMessageRepository
{
    Task AddOrThrowAsync(TelegramUpdateDto telegramUpdate);
    Task AddOrThrowAsync(IEnumerable<TelegramUpdateDto> inputMessages);
    Task<IEnumerable<TelegramUpdateDto>> GetAllOrThrowAsync();
    Task<IEnumerable<TelegramUpdateDto>> GetAllOrThrowAsync(TelegramUserId userId);
    Task HardDeleteAllOrThrowAsync(TelegramUserId userId);
}