using System.Collections.Immutable;
using CheckMade.Common.LangExt;

namespace CheckMade.Common.Model;

public record EnumUiStringProvider
{
    private readonly ImmutableDictionary<EnumCallbackId, UiString>.Builder _promptsBuilder = 
        ImmutableDictionary.CreateBuilder<EnumCallbackId, UiString>();
    
    private readonly ImmutableDictionary<EnumCallbackId, UiString>.Builder _categoryBuilder = 
        ImmutableDictionary.CreateBuilder<EnumCallbackId, UiString>();

    public IReadOnlyDictionary<EnumCallbackId, UiString> ByControlPromptId { get; }
    public IReadOnlyDictionary<EnumCallbackId, UiString> ByDomainCategoryId { get; }

    public EnumUiStringProvider()
    {
        AddPrompt(ControlPrompts.No, Ui("☒ No"));
        AddPrompt(ControlPrompts.Yes, Ui("☑ Yes"));
        AddPrompt(ControlPrompts.Bad, Ui("👎 Bad"));
        AddPrompt(ControlPrompts.Ok, Ui("😐 Ok"));
        AddPrompt(ControlPrompts.Good, Ui("👍 Good"));

        ByControlPromptId = _promptsBuilder.ToImmutable();
        
        AddCategory(DomainCategory.ProblemTypeCleanliness, Ui("❗🪣 Cleanliness"));
        AddCategory(DomainCategory.ProblemTypeTechnical, Ui("❗🔧 Technical"));
        AddCategory(DomainCategory.ProblemTypeConsumable, Ui("🗄 Consumables"));

        ByDomainCategoryId = _categoryBuilder.ToImmutable();
    }
    
    private void AddPrompt(ControlPrompts prompt, UiString uiString) =>
        _promptsBuilder.Add(new EnumCallbackId((int)prompt), uiString);
    
    private void AddCategory(DomainCategory category, UiString uiString) =>
        _categoryBuilder.Add(new EnumCallbackId((int)category), uiString);
}
