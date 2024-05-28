using System.Collections.Immutable;
using CheckMade.Common.LangExt;

namespace CheckMade.Telegram.Model.BotPrompts;

public record BotPromptsDefinition
{
    private readonly ImmutableHashSet<BotPrompt>.Builder _builder = ImmutableHashSet.CreateBuilder<BotPrompt>();
    
    public IReadOnlySet<BotPrompt> AvailableBotResponsePrompts { get; }

    public BotPromptsDefinition()
    {
        Add(Ui("TestOp1"), "op1", BotType.Submissions);
        Add(Ui("TestOp2"), "op2", BotType.Submissions, BotType.Communications);

        AvailableBotResponsePrompts = _builder.ToImmutable();
    }

    private void Add(UiString opText, string opId, params BotType[] supportedBotTypes) =>
        _builder.Add(new BotPrompt(opText, opId, supportedBotTypes));
}
