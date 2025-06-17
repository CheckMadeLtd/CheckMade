using CheckMade.Abstract.Domain.Data.ChatBot.UserInteraction;
using CheckMade.Abstract.Domain.Interfaces.Data.Core;

namespace CheckMade.Abstract.Domain.Data.ChatBot.Output;

public sealed record LogicalPort(IRoleInfo Role, InteractionMode InteractionMode);