using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface ISubmissionsRequestProcessor : IRequestProcessor;

public class SubmissionsRequestProcessor(IMessageRepository repo) : ISubmissionsRequestProcessor
{
    public async Task<string> EchoAsync(InputMessage message)
    {
        await repo.AddAsync(message);

        return message.Details.AttachmentType.Match(
            type => $"Echo from bot Submissions: {type}",
            () => $"Echo from bot Submissions: {message.Details.Text.GetValueOrDefault()}"
        );
    }
}