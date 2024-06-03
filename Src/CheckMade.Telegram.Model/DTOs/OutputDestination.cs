using CheckMade.Common.Model;
using CheckMade.Common.Model.Telegram.Updates;

namespace CheckMade.Telegram.Model.DTOs;

public record OutputDestination(BotType ReceivingBot, Role ReceivingRole);