using System.Collections.Immutable;
using CheckMade.Common.LangExt;

namespace CheckMade.Telegram.Model.BotPrompts;

public record BotPromptsDefinition
{
    private readonly ImmutableDictionary<BotPromptId, UiString>.Builder _builder = 
        ImmutableDictionary.CreateBuilder<BotPromptId, UiString>();
    
    public IReadOnlyDictionary<BotPromptId, UiString> AllBotPrompts { get; }

    public BotPromptsDefinition()
    {
        Add(BotPrompts.No, Ui("No"));
        Add(BotPrompts.Yes, Ui("Yes"));

        AllBotPrompts = _builder.ToImmutable();
    }
    
    private void Add(BotPrompts botPrompt, UiString uiString) =>
        _builder.Add(new BotPromptId((int)botPrompt), uiString);
}
