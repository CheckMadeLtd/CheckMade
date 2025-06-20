using CheckMade.Core.Model.Bot.Categories;
using CheckMade.Core.Model.Bot.DTOs.Input;

namespace CheckMade.Core.Model.Bot.DTOs;

public sealed record Agent(
    UserId UserId,
    ChatId ChatId,
    InteractionMode Mode);