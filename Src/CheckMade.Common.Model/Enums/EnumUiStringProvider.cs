using System.Collections.Immutable;
using CheckMade.Common.LangExt;

namespace CheckMade.Common.Model.Enums;

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
        AddPrompt(ControlPrompts.Back, Ui("🔙 Back"));
        AddPrompt(ControlPrompts.Cancel, Ui("❌ Cancel"));
        AddPrompt(ControlPrompts.Skip, Ui("⏭️ Skip"));
        AddPrompt(ControlPrompts.Save, Ui("💾 Save"));
        AddPrompt(ControlPrompts.Submit, Ui("📤 Submit"));
        AddPrompt(ControlPrompts.Review, Ui("📋 Review"));
        AddPrompt(ControlPrompts.Edit, Ui("✏️ Edit details"));
        AddPrompt(ControlPrompts.Wait, Ui("⏳ Wait..."));
        
        AddPrompt(ControlPrompts.No, Ui("🚫 No"));
        AddPrompt(ControlPrompts.Yes, Ui("✅ Yes"));
        AddPrompt(ControlPrompts.Maybe, Ui("❓ Maybe"));
        
        AddPrompt(ControlPrompts.Bad, Ui("👎 Bad"));
        AddPrompt(ControlPrompts.Ok, Ui("😐 Ok"));
        AddPrompt(ControlPrompts.Good, Ui("👍 Good"));

        ByControlPromptId = _promptsBuilder.ToImmutable();
        
        AddCategory(DomainCategory.SanitaryOpsRoleAdmin, Ui("Sanitary Operations: Admin"));
        AddCategory(DomainCategory.SanitaryOpsRoleInspector, Ui("Sanitary Operations: Inspector"));
        AddCategory(DomainCategory.SanitaryOpsRoleEngineer, Ui("Sanitary Operations: Engineer"));
        AddCategory(DomainCategory.SanitaryOpsRoleCleanLead, Ui("Sanitary Operations: CleanLead"));
        AddCategory(DomainCategory.SanitaryOpsRoleObserver, Ui("Sanitary Operations: Observer"));
        
        AddCategory(DomainCategory.SanitaryOpsIssueCleanliness, Ui("❗🪣 Cleanliness"));
        AddCategory(DomainCategory.SanitaryOpsIssueTechnical, Ui("❗🔧 Technical"));
        AddCategory(DomainCategory.SanitaryOpsIssueConsumable, Ui("❗🗄 Consumables"));
        
        AddCategory(DomainCategory.SanitaryOpsConsumableToiletPaper, Ui("🧻 Toilet Paper"));
        AddCategory(DomainCategory.SanitaryOpsConsumablePaperTowels, Ui("🌫️ Paper Towels"));
        AddCategory(DomainCategory.SanitaryOpsConsumableSoap, Ui("🧴 Soap"));
        
        AddCategory(DomainCategory.SanitaryOpsFacilityToilets, Ui("🚽 Toilets"));
        AddCategory(DomainCategory.SanitaryOpsFacilityShowers, Ui("🚿 Showers"));
        AddCategory(DomainCategory.SanitaryOpsFacilityStaff, Ui("🙋 Staff"));
        AddCategory(DomainCategory.SanitaryOpsFacilityOther, Ui("Other Facility"));

        ByDomainCategoryId = _categoryBuilder.ToImmutable();
    }
    
    private void AddPrompt(ControlPrompts prompt, UiString uiString) =>
        _promptsBuilder.Add(new EnumCallbackId((long)prompt), uiString);
    
    private void AddCategory(DomainCategory category, UiString uiString) =>
        _categoryBuilder.Add(new EnumCallbackId((int)category), uiString);
}
