using System.ComponentModel;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface IRequestProcessorSelector
{
    IRequestProcessor GetRequestProcessor(BotType botType);
}

public class RequestProcessorSelector(
        ISubmissionsRequestProcessor submissionsProcessor,
        ICommunicationsRequestProcessor communicationsProcessor,
        INotificationsRequestProcessor notificationsProcessor) 
    : IRequestProcessorSelector
{
    public IRequestProcessor GetRequestProcessor(BotType botType)
    {
        return botType switch
        {
            BotType.Submissions => submissionsProcessor,
            BotType.Communications => communicationsProcessor,
            BotType.Notifications => notificationsProcessor,
            _ => throw new InvalidEnumArgumentException()
        };
    }
}