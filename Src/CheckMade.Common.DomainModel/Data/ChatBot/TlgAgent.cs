using CheckMade.Common.DomainModel.Data.ChatBot.Input;
using CheckMade.Common.DomainModel.Data.ChatBot.UserInteraction;

namespace CheckMade.Common.DomainModel.Data.ChatBot;

public sealed record TlgAgent(
    TlgUserId UserId,
    TlgChatId ChatId,
    InteractionMode Mode);