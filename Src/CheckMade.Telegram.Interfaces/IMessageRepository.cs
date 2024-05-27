using CheckMade.Telegram.Model.DTOs;

namespace CheckMade.Telegram.Interfaces;

public interface IMessageRepository
{
    Task AddOrThrowAsync(InputMessageDto inputMessage);
    Task AddOrThrowAsync(IEnumerable<InputMessageDto> inputMessages);
    Task<IEnumerable<InputMessageDto>> GetAllOrThrowAsync();
    Task<IEnumerable<InputMessageDto>> GetAllOrThrowAsync(long userId);
    Task HardDeleteAllOrThrowAsync(long userId);
}