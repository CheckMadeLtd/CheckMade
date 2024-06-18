using System.Collections.Immutable;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using static CheckMade.Common.Model.Core.DomainCategories;

namespace CheckMade.Common.Model.Utils;

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
        
        // AddCategory(SanitaryOpsFacility.SanitaryOps_IssueCleanliness, Ui("🪣 Cleanliness"));
        // AddCategory(SanitaryOpsFacility.SanitaryOps_IssueTechnical, Ui("🔧 Technical"));
        // AddCategory(SanitaryOpsFacility.SanitaryOps_IssueConsumable, Ui("🗄 Consumables"));
        //
        // AddCategory(SanitaryOpsFacility.SanitaryOps_ConsumableToiletPaper, Ui("🧻 Toilet Paper"));
        // AddCategory(SanitaryOpsFacility.SanitaryOps_ConsumablePaperTowels, Ui("🌫️ Paper Towels"));
        // AddCategory(SanitaryOpsFacility.SanitaryOps_ConsumableSoap, Ui("🧴 Soap"));
        
        AddCategory(SanitaryOpsFacility.SanitaryOps_FacilityToilets, Ui("🚽 Toilets"));
        AddCategory(SanitaryOpsFacility.SanitaryOps_FacilityShowers, Ui("🚿 Showers"));
        AddCategory(SanitaryOpsFacility.SanitaryOps_FacilityStaff, Ui("🙋 Staff"));
        AddCategory(SanitaryOpsFacility.SanitaryOps_FacilityOther, Ui("Other Facility"));

        ByDomainCategoryId = _categoryBuilder.ToImmutable();
    }
    
    private void AddPrompt(ControlPrompts prompt, UiString uiString) =>
        _promptsBuilder.Add(new EnumCallbackId((long)prompt), uiString);
    
    private void AddCategory(SanitaryOpsFacility category, UiString uiString) =>
        _categoryBuilder.Add(new EnumCallbackId((int)category), uiString);
}
