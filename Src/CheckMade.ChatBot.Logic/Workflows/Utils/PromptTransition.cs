using CheckMade.Abstract.Domain.Data.ChatBot;
using CheckMade.Abstract.Domain.Data.ChatBot.Output;
using CheckMade.Abstract.Domain.Interfaces.ChatBot.Function;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.ChatBot.Logic.Workflows.Utils;

internal sealed record PromptTransition
{
    /// <summary>
    /// Overwrites the bot's current message with the next one 
    /// </summary>
    internal PromptTransition(bool isNextPromptInPlaceUpdate)
    {
        IsNextPromptInPlaceUpdate = isNextPromptInPlaceUpdate;
        CurrentPromptFinalizer = Option<Output>.None();
    }

    /// <summary>
    /// Explicitly specifies how to finalize (modify) the bot's current message, before sending the next one.
    /// E.g. showing the original prompt plus the user's choice from among DomainTerms, while removing InlineKeyboard. 
    /// </summary>
    internal PromptTransition(Output currentPromptFinalizer)
    {
        IsNextPromptInPlaceUpdate = false;
        CurrentPromptFinalizer = currentPromptFinalizer;
    }

    /// <summary>
    /// Uses default finalization of bot's current message: just removing any InlineKeyboard buttons.
    /// Optionally applies to last message: e.g. in cases where it has an InlineKeyboard that wasn't used. 
    /// </summary>
    internal PromptTransition(
        MessageId currentMessageId,
        ILastOutputMessageIdCache msgIdCache,
        Agent currentAgent,
        bool applyToPreviousInsteadOfCurrentInput = false)
    {
        IsNextPromptInPlaceUpdate = false;

        var updateMessageId = applyToPreviousInsteadOfCurrentInput
            ? msgIdCache.GetLastMessageId(currentAgent)
            : currentMessageId;

        CurrentPromptFinalizer = new Output
        {
            UpdateExistingOutputMessageId = updateMessageId
        };
    }
    
    internal bool IsNextPromptInPlaceUpdate { get; }
    internal Option<Output> CurrentPromptFinalizer { get; }
}