using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete.RoleTypes;

public class TradeTeamLead<T> : ITradeRoleType<T> where T : ITrade, new()
{
    public Option<ITrade> GetTrade() => new T();
}