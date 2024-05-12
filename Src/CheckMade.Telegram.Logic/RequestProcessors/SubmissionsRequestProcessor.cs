using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface ISubmissionsRequestProcessor : IRequestProcessor;

public class SubmissionsRequestProcessor(IMessageRepository repo) : ISubmissionsRequestProcessor
{
    public async Task<string> EchoAsync(InputMessage message)
    {
        await repo.AddAsync(message);

        var attachmentType = message.Details.AttachmentType; 
        
        return attachmentType switch
        {
            AttachmentType.NotApplicable => $"Echo from bot Submissions: {message.Details.Text}",
            _ => $"Echo from bot Submissions: {attachmentType}"
        };
    }
}