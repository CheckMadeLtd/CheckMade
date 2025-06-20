using CheckMade.Abstract.Domain.Model.Bot.Categories;
using CheckMade.Abstract.Domain.Model.Bot.DTOs.Input;

namespace CheckMade.Abstract.Domain.Model.Bot.DTOs;

public sealed record Agent(
    UserId UserId,
    ChatId ChatId,
    InteractionMode Mode);