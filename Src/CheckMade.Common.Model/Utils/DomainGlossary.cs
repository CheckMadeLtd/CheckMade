using CheckMade.Common.Model.Core.SanitaryOps.Facilities;
using CheckMade.Common.Model.Core.SanitaryOps.Issues;

namespace CheckMade.Common.Model.Utils;

public static class DomainGlossary
{
    public static IDictionary<OneOf<int, Type>, (string callbackId, UiString uiString)> 
        CallbackIdAndUiStringByDomainCategory { get; }
        = new Dictionary<OneOf<int, Type>, (string callbackId, UiString uiString)>
        {
            { typeof(CleanlinessIssue), ("AWYZP", Ui("🪣 Cleanliness")) },
            { typeof(TechnicalIssue), ("M46NG", Ui("🔧 Technical")) },
            { typeof(ConsumablesIssue), ("582QJ", Ui("🗄 Consumables")) },
            
            { (int)ConsumablesIssue.Item.ToiletPaper, ("STP1N", Ui("🧻 Toilet Paper")) },
            { (int)ConsumablesIssue.Item.PaperTowels, ("OJH85", Ui("🌫️ Paper Towels")) },
            { (int)ConsumablesIssue.Item.Soap, ("79AMO", Ui("🧴 Soap")) },
            
            { typeof(Toilet), ("1540N", Ui("🚽 Toilet")) },
            { typeof(Shower), ("4W2GW", Ui("🚿 Shower")) },
            { typeof(Staff), ("9MRJ9", Ui("🙋 Staff")) },
        };

    public static IDictionary<string, OneOf<int, Type>> DomainCategoryByCallbackId { get; }
        = CallbackIdAndUiStringByDomainCategory.ToDictionary(
            kvp => kvp.Value.callbackId,
            kvp => kvp.Key);
}
