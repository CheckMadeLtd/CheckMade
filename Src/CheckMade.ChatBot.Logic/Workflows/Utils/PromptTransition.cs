using CheckMade.Common.Domain.Data.ChatBot;
using CheckMade.Common.Domain.Data.ChatBot.Output;
using CheckMade.Common.Domain.Interfaces.ChatBot.Function;
using CheckMade.Common.Utils.FpExtensions.Monads;

namespace CheckMade.ChatBot.Logic.Workflows.Utils;

internal sealed record PromptTransition
{
    /// <summary>
    /// Overwrites the bot's current message with the next one 
    /// </summary>
    internal PromptTransition(bool isNextPromptInPlaceUpdate)
    {
        IsNextPromptInPlaceUpdate = isNextPromptInPlaceUpdate;
        CurrentPromptFinalizer = Option<OutputDto>.None();
    }

    /// <summary>
    /// Explicitly specifies how to finalize (modify) the bot's current message, before sending the next one.
    /// E.g. showing the original prompt plus the user's choice from among DomainTerms, while removing InlineKeyboard. 
    /// </summary>
    internal PromptTransition(OutputDto currentPromptFinalizer)
    {
        IsNextPromptInPlaceUpdate = false;
        CurrentPromptFinalizer = currentPromptFinalizer;
    }

    /// <summary>
    /// Uses default finalization of bot's current message: just removing any InlineKeyboard buttons.
    /// Optionally applies to last message: e.g. in cases where it has an InlineKeyboard that wasn't used. 
    /// </summary>
    internal PromptTransition(
        TlgMessageId currentMessageId,
        ILastOutputMessageIdCache msgIdCache,
        TlgAgent currentTlgAgent,
        bool applyToPreviousInsteadOfCurrentInput = false)
    {
        IsNextPromptInPlaceUpdate = false;

        var updateMessageId = applyToPreviousInsteadOfCurrentInput
            ? msgIdCache.GetLastMessageId(currentTlgAgent)
            : currentMessageId;

        CurrentPromptFinalizer = new OutputDto
        {
            UpdateExistingOutputMessageId = updateMessageId
        };
    }
    
    internal bool IsNextPromptInPlaceUpdate { get; }
    internal Option<OutputDto> CurrentPromptFinalizer { get; }
}