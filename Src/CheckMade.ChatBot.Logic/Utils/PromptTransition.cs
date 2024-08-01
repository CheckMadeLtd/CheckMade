using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Utils;

internal sealed record PromptTransition
{
    internal PromptTransition(bool isNextPromptInPlaceUpdate)
    {
        IsNextPromptInPlaceUpdate = isNextPromptInPlaceUpdate;
        CurrentPromptFinalizer = Option<OutputDto>.None();
    }

    internal PromptTransition(OutputDto currentPromptFinalizer)
    {
        IsNextPromptInPlaceUpdate = false;
        CurrentPromptFinalizer = currentPromptFinalizer;
    }
    
    internal bool IsNextPromptInPlaceUpdate { get; }
    internal Option<OutputDto> CurrentPromptFinalizer { get; }
}