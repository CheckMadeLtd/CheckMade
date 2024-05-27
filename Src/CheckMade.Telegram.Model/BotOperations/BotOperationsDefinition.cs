using System.Collections.Immutable;
using CheckMade.Common.LangExt;

namespace CheckMade.Telegram.Model.BotOperations;

public record BotOperationsDefinition
{
    private readonly ImmutableHashSet<BotOperation>.Builder _builder = ImmutableHashSet.CreateBuilder<BotOperation>();
    
    public IReadOnlySet<BotOperation> AvailableBotOperations { get; }

    public BotOperationsDefinition()
    {
        Add(Ui("TestOp1"), "op1", BotType.Submissions);
        Add(Ui("TestOp2"), "op2", BotType.Submissions, BotType.Communications);

        AvailableBotOperations = _builder.ToImmutable();
    }

    private void Add(UiString opText, string opId, params BotType[] supportedBotTypes) =>
        _builder.Add(new BotOperation(opText, opId, supportedBotTypes));
}
