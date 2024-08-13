using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Actors.RoleSystem;

namespace CheckMade.Common.Model.ChatBot.Output;

public sealed record LogicalPort(IRoleInfo Role, InteractionMode InteractionMode);