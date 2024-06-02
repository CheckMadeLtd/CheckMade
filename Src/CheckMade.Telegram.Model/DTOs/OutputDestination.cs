using CheckMade.Common.Model;

namespace CheckMade.Telegram.Model.DTOs;

public record OutputDestination(BotType ReceivingBot, Role ReceivingRole);