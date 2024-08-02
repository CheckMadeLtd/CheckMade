using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Utils;

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
    internal PromptTransition(int currentMessageId, bool applyToPreviousInsteadOfCurrentInput = false)
    {
        IsNextPromptInPlaceUpdate = false;

        var updateMessageId = applyToPreviousInsteadOfCurrentInput
            ? currentMessageId - 1
            : currentMessageId;

        CurrentPromptFinalizer = new OutputDto
        {
            UpdateExistingOutputMessageId = updateMessageId
        };
    }
    
    internal bool IsNextPromptInPlaceUpdate { get; }
    internal Option<OutputDto> CurrentPromptFinalizer { get; }
}