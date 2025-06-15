using CheckMade.Common.Domain.Data.ChatBot.Input;
using CheckMade.Common.Domain.Data.ChatBot.UserInteraction;

namespace CheckMade.Common.Domain.Data.ChatBot;

public sealed record TlgAgent(
    TlgUserId UserId,
    TlgChatId ChatId,
    InteractionMode Mode);