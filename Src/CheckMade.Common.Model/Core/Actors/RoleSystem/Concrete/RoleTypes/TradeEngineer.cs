using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete.RoleTypes;

public class TradeEngineer<T> : ITradeRoleType<T> where T : ITrade, new()
{
    public Option<ITrade> GetTradeParameter() => new T();
}