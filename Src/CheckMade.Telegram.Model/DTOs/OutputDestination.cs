using CheckMade.Common.Model;
using CheckMade.Common.Model.TelegramUpdates;

namespace CheckMade.Telegram.Model.DTOs;

public record OutputDestination(BotType ReceivingBot, Role ReceivingRole);