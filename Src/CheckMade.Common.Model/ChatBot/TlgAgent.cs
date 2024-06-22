using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;

namespace CheckMade.Common.Model.ChatBot;

public record TlgAgent(
    TlgUserId UserId,
    TlgChatId ChatId,
    InteractionMode Mode);