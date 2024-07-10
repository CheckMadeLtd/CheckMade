using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.Actors.RoleSystem.RoleTypes;

public class TradeObserver<T> : ITradeRoleType<T> where T : ITrade;