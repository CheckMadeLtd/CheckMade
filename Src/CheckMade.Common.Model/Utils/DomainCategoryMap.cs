using static CheckMade.Common.Model.Core.DomainCategories;

namespace CheckMade.Common.Model.Utils;

public static class DomainCategoryMap
{
    public static IDictionary<object, (string callbackId, UiString uiString)> CallbackIdAndUiStringByDomainCategory { get; }
        = new Dictionary<object, (string callbackId, UiString uiString)>
        {
            { SanitaryOpsIssue.Cleanliness, ("AWYZP", Ui("🪣 Cleanliness"))},
            { SanitaryOpsIssue.Technical, ("M46NG", Ui("🔧 Technical"))},
            { SanitaryOpsIssue.Consumable, ("582QJ", Ui("🗄 Consumables"))},
        };

    public static IDictionary<string, object> DomainCategoryByCallbackId { get; }
        = CallbackIdAndUiStringByDomainCategory.ToDictionary(
            kvp => kvp.Value.callbackId,
            kvp => kvp.Key);
};



// AddCategory(SanitaryOpsIssue.Cleanliness, Ui("🪣 Cleanliness"));
// AddCategory(SanitaryOpsIssue.Technical, Ui("🔧 Technical"));
// AddCategory(SanitaryOpsIssue.Consumable, Ui("🗄 Consumables"));
//             
// AddCategory(SanitaryOpsConsumable.ToiletPaper, Ui("🧻 Toilet Paper"));
// AddCategory(SanitaryOpsConsumable.PaperTowels, Ui("🌫️ Paper Towels"));
// AddCategory(SanitaryOpsConsumable.Soap, Ui("🧴 Soap"));
//             
// AddCategory(SanitaryOpsFacility.Toilets, Ui("🚽 Toilets"));
// AddCategory(SanitaryOpsFacility.Showers, Ui("🚿 Showers"));
// AddCategory(SanitaryOpsFacility.Staff, Ui("🙋 Staff"));
// AddCategory(SanitaryOpsFacility.Other, Ui("Other Facility"));
