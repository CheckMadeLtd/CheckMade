using CheckMade.Common.Model.Core.Trades.Types;

namespace CheckMade.Common.Model.Core.LiveEvents;

public record SphereOfAction
{
    public SphereOfAction(string name, Type tradeType)
    {
        var expectedNameSpaceForTradeType = typeof(SanitaryOps).Namespace;
        
        if (tradeType.Namespace is null || 
            tradeType.Namespace != expectedNameSpaceForTradeType)
        {
            throw new ArgumentException($"Invalid namespace for {nameof(tradeType)}. " +
                                        $"Is: {tradeType.Namespace}, but must be: {expectedNameSpaceForTradeType}");
        }

        Name = name;
        TradeType = tradeType;
    }

    public string Name { get; init; }
    public Type TradeType { get; init; }
}