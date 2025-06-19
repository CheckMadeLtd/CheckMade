using CheckMade.Abstract.Domain.Data.Bot.Input;
using CheckMade.Abstract.Domain.Data.Bot.UserInteraction;

namespace CheckMade.Abstract.Domain.Data.Bot;

public sealed record Agent(
    UserId UserId,
    ChatId ChatId,
    InteractionMode Mode);