using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface ISubmissionsRequestProcessor : IRequestProcessor;

public class SubmissionsRequestProcessor(IMessageRepository repo) : ISubmissionsRequestProcessor
{
    public async Task<string> EchoAsync(InputMessage message)
    {
        await repo.AddAsync(message);

        return message.Details.AttachmentType switch
        {
            AttachmentType.NotApplicable => $"Echo from bot Submissions: {message.Details.Text}",
            AttachmentType.Photo => $"Echo from bot Submissions: photo",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}