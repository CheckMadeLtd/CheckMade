using System.Collections.Immutable;
using CheckMade.Common.LangExt;

namespace CheckMade.Telegram.Model.ControlPrompt;

public record ControlPromptsProvider
{
    private readonly ImmutableDictionary<ControlPromptCallbackId, UiString>.Builder _builder = 
        ImmutableDictionary.CreateBuilder<ControlPromptCallbackId, UiString>();
    
    public IReadOnlyDictionary<ControlPromptCallbackId, UiString> UiById { get; }

    public ControlPromptsProvider()
    {
        Add(ControlPrompts.No, Ui("☒ No"));
        Add(ControlPrompts.Yes, Ui("☑ Yes"));
        Add(ControlPrompts.Bad, Ui("👎 Bad"));
        Add(ControlPrompts.Ok, Ui("😐 Ok"));
        Add(ControlPrompts.Good, Ui("👍 Good"));
        Add(ControlPrompts.ProblemTypeCleanliness, Ui("❗🪣 Cleanliness"));
        Add(ControlPrompts.ProblemTypeTechnical, Ui("❗🔧 Technical"));
        Add(ControlPrompts.ProblemTypeConsumable, Ui("🗄 Consumables"));

        UiById = _builder.ToImmutable();
    }
    
    private void Add(ControlPrompts prompt, UiString uiString) =>
        _builder.Add(new ControlPromptCallbackId((int)prompt), uiString);
}
