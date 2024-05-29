using System.Collections.Immutable;
using CheckMade.Common.LangExt;

namespace CheckMade.Telegram.Model.BotPrompts;

public record BotPromptsDefinition
{
    private readonly ImmutableHashSet<ModelBotPrompt>.Builder _builder = ImmutableHashSet.CreateBuilder<ModelBotPrompt>();
    
    public IReadOnlySet<ModelBotPrompt> AvailableBotResponsePrompts { get; }

    public BotPromptsDefinition()
    {
        Add(Ui("Problem type?"), "problem_type");
        Add(Ui("TestPrompt"), "test_prompt");

        AvailableBotResponsePrompts = _builder.ToImmutable();
    }

    private void Add(UiString text, string id) =>
        _builder.Add(new ModelBotPrompt(text, id));
}
