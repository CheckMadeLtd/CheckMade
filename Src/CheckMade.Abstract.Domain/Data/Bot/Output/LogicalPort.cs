using CheckMade.Abstract.Domain.Data.Bot.UserInteraction;
using CheckMade.Abstract.Domain.Interfaces.Data.Core;

namespace CheckMade.Abstract.Domain.Data.Bot.Output;

public sealed record LogicalPort(IRoleInfo Role, InteractionMode InteractionMode);