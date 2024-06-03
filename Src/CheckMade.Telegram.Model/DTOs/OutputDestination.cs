using CheckMade.Common.Model;
using CheckMade.Common.Model.Telegram.Updates;

namespace CheckMade.Telegram.Model.DTOs;

public record OutputDestination(BotType DestinationBotType, Role DestinationRole);