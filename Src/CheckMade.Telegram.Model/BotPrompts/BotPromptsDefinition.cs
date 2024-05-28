using System.Collections.Immutable;
using CheckMade.Common.LangExt;

namespace CheckMade.Telegram.Model.BotPrompts;

public record BotPromptsDefinition
{
    private readonly ImmutableHashSet<BotPrompt>.Builder _builder = ImmutableHashSet.CreateBuilder<BotPrompt>();
    
    public IReadOnlySet<BotPrompt> AvailableBotResponsePrompts { get; }

    public BotPromptsDefinition()
    {
        Add(Ui("Problem type?"), "problem_type", BotType.Submissions);
        Add(Ui("TestPrompt"), "test_prompt", BotType.Submissions, BotType.Communications);

        AvailableBotResponsePrompts = _builder.ToImmutable();
    }

    private void Add(UiString text, string id, params BotType[] supportedBotTypes) =>
        _builder.Add(new BotPrompt(text, id, supportedBotTypes));
}
