using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Telegram.UserInteraction;

namespace CheckMade.Common.Model.Telegram;

public record LogicalPort(Role Role, InteractionMode InteractionMode);