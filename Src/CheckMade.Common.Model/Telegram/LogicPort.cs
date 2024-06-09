using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Telegram.UserInteraction;

namespace CheckMade.Common.Model.Telegram;

public record LogicPort(Role Role, InteractionMode InteractionMode);