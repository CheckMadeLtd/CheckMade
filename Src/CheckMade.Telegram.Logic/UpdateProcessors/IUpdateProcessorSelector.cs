using CheckMade.Common.Model.Tlg.Updates;
using CheckMade.Telegram.Logic.UpdateProcessors.Concrete;

namespace CheckMade.Telegram.Logic.UpdateProcessors;

public interface IUpdateProcessorSelector
{
    IUpdateProcessor GetUpdateProcessor(TlgBotType botType);
}

public class UpdateProcessorSelector(
        IOperationsUpdateProcessor operationsProcessor,
        ICommunicationsUpdateProcessor communicationsProcessor,
        INotificationsUpdateProcessor notificationsProcessor) 
    : IUpdateProcessorSelector
{
    public IUpdateProcessor GetUpdateProcessor(TlgBotType botType)
    {
        return botType switch
        {
            TlgBotType.Operations => operationsProcessor,
            TlgBotType.Communications => communicationsProcessor,
            TlgBotType.Notifications => notificationsProcessor,
            _ => throw new ArgumentOutOfRangeException(nameof(botType))
        };
    }
}