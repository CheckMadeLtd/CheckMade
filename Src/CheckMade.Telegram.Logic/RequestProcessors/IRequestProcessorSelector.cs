using CheckMade.Common.Model.Telegram.Updates;
using CheckMade.Telegram.Logic.RequestProcessors.Concrete;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface IRequestProcessorSelector
{
    IRequestProcessor GetRequestProcessor(BotType botType);
}

public class RequestProcessorSelector(
        IOperationsRequestProcessor operationsProcessor,
        ICommunicationsRequestProcessor communicationsProcessor,
        INotificationsRequestProcessor notificationsProcessor) 
    : IRequestProcessorSelector
{
    public IRequestProcessor GetRequestProcessor(BotType botType)
    {
        return botType switch
        {
            BotType.Operations => operationsProcessor,
            BotType.Communications => communicationsProcessor,
            BotType.Notifications => notificationsProcessor,
            _ => throw new ArgumentOutOfRangeException(nameof(botType))
        };
    }
}