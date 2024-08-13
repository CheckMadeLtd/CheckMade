using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;

namespace CheckMade.Common.Model.ChatBot;

public sealed record TlgAgent(
    TlgUserId UserId,
    TlgChatId ChatId,
    InteractionMode Mode);