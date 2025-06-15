using CheckMade.Common.DomainModel.Data.ChatBot.UserInteraction;
using CheckMade.Common.DomainModel.Interfaces.Data.Core;

namespace CheckMade.Common.DomainModel.Data.ChatBot.Output;

public sealed record LogicalPort(IRoleInfo Role, InteractionMode InteractionMode);