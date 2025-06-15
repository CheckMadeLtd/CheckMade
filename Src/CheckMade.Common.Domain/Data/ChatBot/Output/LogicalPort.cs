using CheckMade.Common.Domain.Data.ChatBot.UserInteraction;
using CheckMade.Common.Domain.Interfaces.Data.Core;

namespace CheckMade.Common.Domain.Data.ChatBot.Output;

public sealed record LogicalPort(IRoleInfo Role, InteractionMode InteractionMode);