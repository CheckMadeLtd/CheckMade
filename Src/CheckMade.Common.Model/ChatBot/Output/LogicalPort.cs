using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Actors.RoleSystem;

namespace CheckMade.Common.Model.ChatBot.Output;

public record LogicalPort(Role Role, InteractionMode InteractionMode);