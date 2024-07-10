using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.Actors.RoleSystem.RoleTypes;

public class TradeAdmin<T> : ITradeRoleType<T> where T : ITrade;