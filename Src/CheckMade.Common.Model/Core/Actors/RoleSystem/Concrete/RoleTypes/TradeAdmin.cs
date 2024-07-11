using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete.RoleTypes;

public class TradeAdmin<T> : ITradeRoleType<T> where T : ITrade, new()
{
    public Option<ITrade> GetTradeInstance() => new T();
    public Option<Type> GetTradeType() => typeof(T);
}