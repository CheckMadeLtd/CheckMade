using CheckMade.Common.DomainModel.ChatBot.UserInteraction;
using CheckMade.Common.DomainModel.Interfaces.Core;

namespace CheckMade.Common.DomainModel.ChatBot.Output;

public sealed record LogicalPort(IRoleInfo Role, InteractionMode InteractionMode);