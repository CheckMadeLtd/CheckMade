using CheckMade.Common.Domain.Data.ChatBot.Input;
using CheckMade.Common.Domain.Data.ChatBot.UserInteraction;

namespace CheckMade.Common.Domain.Data.ChatBot;

public sealed record Agent(
    UserId UserId,
    ChatId ChatId,
    InteractionMode Mode);