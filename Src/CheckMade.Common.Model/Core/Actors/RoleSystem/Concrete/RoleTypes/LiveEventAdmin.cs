using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete.RoleTypes;

public class LiveEventAdmin : ILiveEventRoleType
{
    public Option<ITrade> GetTradeParameter() => Option<ITrade>.None();
}