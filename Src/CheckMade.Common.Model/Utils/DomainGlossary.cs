using System.Collections.Immutable;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.SanitaryOps.Facilities;
using CheckMade.Common.Model.Core.SanitaryOps.Issues;

namespace CheckMade.Common.Model.Utils;

public class DomainGlossary
{
    private readonly ImmutableDictionary<OneOf<EnumWithType, Type>, (CallbackId callbackId, UiString uiString)>.Builder
        _domainGlossaryBuilder =
            ImmutableDictionary.CreateBuilder<OneOf<EnumWithType, Type>, (CallbackId callbackId, UiString uiString)>();
    
    public IReadOnlyDictionary<OneOf<EnumWithType, Type>, (CallbackId callbackId, UiString uiString)> IdAndUiByTerm { get; }

    public IDictionary<CallbackId, OneOf<EnumWithType, Type>> TermById { get; }
    
    public DomainGlossary()
    {
        AddTerm(typeof(CleanlinessIssue), "DAWYZP", Ui("🪣 Cleanliness"));
        AddTerm(typeof(TechnicalIssue), "DM46NG", Ui("🔧 Technical"));
        AddTerm(typeof(ConsumablesIssue), "D582QJ", Ui("🗄 Consumables"));

        AddTerm(Et(ConsumablesIssue.Item.ToiletPaper), "DSTP1N", Ui("🧻 Toilet Paper"));
        AddTerm(Et(ConsumablesIssue.Item.PaperTowels), "DOJH85", Ui("🌫️ Paper Towels"));
        AddTerm(Et(ConsumablesIssue.Item.Soap), "D79AMO", Ui("🧴 Soap"));

        AddTerm(typeof(Toilet), "D1540N", Ui("🚽 Toilet"));
        AddTerm(typeof(Shower), "D4W2GW", Ui("🚿 Shower"));
        AddTerm(typeof(Staff), "D9MRJ9", Ui("🙋 Staff"));
        
        AddTerm(Et(LanguageCode.en), "DFVN7W", Ui("🇩🇪 German"));
        AddTerm(Et(LanguageCode.en), "DCQ4ME", Ui("🇬🇧 English"));

        IdAndUiByTerm = _domainGlossaryBuilder.ToImmutable();
        
        TermById = IdAndUiByTerm.ToDictionary(
            kvp => kvp.Value.callbackId,
            kvp => kvp.Key);
    }

    private void AddTerm(OneOf<EnumWithType, Type> term, string idRaw, UiString uiString) =>
        _domainGlossaryBuilder.Add(term, (new CallbackId(idRaw), uiString));
}
