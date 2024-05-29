using System.Collections.Immutable;
using CheckMade.Common.LangExt;

namespace CheckMade.Telegram.Model.BotPrompts;

public record BotPromptsDefinition
{
    private readonly ImmutableDictionary<BotPromptId, UiString>.Builder _builder = 
        ImmutableDictionary.CreateBuilder<BotPromptId, UiString>();
    
    public IReadOnlyDictionary<BotPromptId, UiString> BotPromptUiById { get; }

    public BotPromptsDefinition()
    {
        Add(EBotPrompts.No, Ui("☒ No"));
        Add(EBotPrompts.Yes, Ui("☑ Yes"));
        Add(EBotPrompts.Bad, Ui("👎 Bad"));
        Add(EBotPrompts.Ok, Ui("😐 Ok"));
        Add(EBotPrompts.Good, Ui("👍 Good"));
        Add(EBotPrompts.ProblemType, Ui("Problem type?"));

        BotPromptUiById = _builder.ToImmutable();
    }
    
    private void Add(EBotPrompts botPrompt, UiString uiString) =>
        _builder.Add(new BotPromptId((int)botPrompt), uiString);
}
