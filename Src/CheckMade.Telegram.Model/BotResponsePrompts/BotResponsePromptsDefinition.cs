using System.Collections.Immutable;
using CheckMade.Common.LangExt;

namespace CheckMade.Telegram.Model.BotResponsePrompts;

public record BotResponsePromptsDefinition
{
    private readonly ImmutableHashSet<BotResponsePrompt>.Builder _builder = ImmutableHashSet.CreateBuilder<BotResponsePrompt>();
    
    public IReadOnlySet<BotResponsePrompt> AvailableBotResponsePrompts { get; }

    public BotResponsePromptsDefinition()
    {
        Add(Ui("TestOp1"), "op1", BotType.Submissions);
        Add(Ui("TestOp2"), "op2", BotType.Submissions, BotType.Communications);

        AvailableBotResponsePrompts = _builder.ToImmutable();
    }

    private void Add(UiString opText, string opId, params BotType[] supportedBotTypes) =>
        _builder.Add(new BotResponsePrompt(opText, opId, supportedBotTypes));
}
