using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.Actors.RoleSystem.RoleTypes;

public class TradeTeamLead<T> : ITradeRoleType<T> where T : ITrade;