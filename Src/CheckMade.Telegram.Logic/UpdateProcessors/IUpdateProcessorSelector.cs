using CheckMade.Common.Model.Telegram.Updates;
using CheckMade.Telegram.Logic.UpdateProcessors.Concrete;

namespace CheckMade.Telegram.Logic.UpdateProcessors;

public interface IUpdateProcessorSelector
{
    IUpdateProcessor GetUpdateProcessor(BotType botType);
}

public class UpdateProcessorSelector(
        IOperationsUpdateProcessor operationsProcessor,
        ICommunicationsUpdateProcessor communicationsProcessor,
        INotificationsUpdateProcessor notificationsProcessor) 
    : IUpdateProcessorSelector
{
    public IUpdateProcessor GetUpdateProcessor(BotType botType)
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