using CheckMade.Abstract.Domain.Data.ChatBot.Input;
using CheckMade.Abstract.Domain.Data.ChatBot.UserInteraction;

namespace CheckMade.Abstract.Domain.Data.ChatBot;

public sealed record Agent(
    UserId UserId,
    ChatId ChatId,
    InteractionMode Mode);