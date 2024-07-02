using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Actors;

namespace CheckMade.Common.Model.ChatBot;

public record LogicalPort(Role Role, InteractionMode InteractionMode);