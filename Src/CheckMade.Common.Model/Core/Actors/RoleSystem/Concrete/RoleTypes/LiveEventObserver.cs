using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete.RoleTypes;

public class LiveEventObserver : ILiveEventRoleType
{
    public Option<ITrade> GetTradeInstance() => Option<ITrade>.None();
    public Option<Type> GetTradeType() => Option<Type>.None();
}