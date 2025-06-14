using CheckMade.Common.DomainModel.ChatBot.Input;
using CheckMade.Common.DomainModel.ChatBot.UserInteraction;

namespace CheckMade.Common.DomainModel.ChatBot;

public sealed record TlgAgent(
    TlgUserId UserId,
    TlgChatId ChatId,
    InteractionMode Mode);