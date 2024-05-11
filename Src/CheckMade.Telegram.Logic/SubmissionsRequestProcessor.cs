using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Logic;

public interface ISubmissionsRequestProcessor : IRequestProcessor;

public class SubmissionsRequestProcessor(IMessageRepository repo) : ISubmissionsRequestProcessor
{
    public async Task<string> EchoAsync(InputMessage message)
    {
        await repo.AddAsync(message);
        return $"Echo from bot Submissions: {message.Details.Text}";
    }
}