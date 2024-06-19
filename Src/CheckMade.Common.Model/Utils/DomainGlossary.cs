using System.Collections.Immutable;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.SanitaryOps.Facilities;
using CheckMade.Common.Model.Core.SanitaryOps.Issues;

namespace CheckMade.Common.Model.Utils;

public class DomainGlossary
{
    private readonly ImmutableDictionary<DomainTerm, (CallbackId callbackId, UiString uiString)>.Builder
        _domainGlossaryBuilder =
            ImmutableDictionary.CreateBuilder<DomainTerm, (CallbackId callbackId, UiString uiString)>();
    
    public IReadOnlyDictionary<DomainTerm, (CallbackId callbackId, UiString uiString)> IdAndUiByTerm { get; }

    public IDictionary<CallbackId, DomainTerm> TermById { get; }
    
    public DomainGlossary()
    {
        AddTerm(Dt(typeof(CleanlinessIssue)), "DAWYZP", Ui("🪣 Cleanliness"));
        AddTerm(Dt(typeof(TechnicalIssue)), "DM46NG", Ui("🔧 Technical"));
        AddTerm(Dt(typeof(ConsumablesIssue)), "D582QJ", Ui("🗄 Consumables"));

        AddTerm(Dt(ConsumablesIssue.Item.ToiletPaper), "DSTP1N", Ui("🧻 Toilet Paper"));
        AddTerm(Dt(ConsumablesIssue.Item.PaperTowels), "DOJH85", Ui("🌫️ Paper Towels"));
        AddTerm(Dt(ConsumablesIssue.Item.Soap), "D79AMO", Ui("🧴 Soap"));

        AddTerm(Dt(typeof(Toilet)), "D1540N", Ui("🚽 Toilet"));
        AddTerm(Dt(typeof(Shower)), "D4W2GW", Ui("🚿 Shower"));
        AddTerm(Dt(typeof(Staff)), "D9MRJ9", Ui("🙋 Staff"));
        
        // AddTerm(Dt(LanguageCode.en), "DFVN7W", Ui("🇩🇪 German"));
        // AddTerm(Dt(LanguageCode.en), "DCQ4ME", Ui("🇬🇧 English"));

        IdAndUiByTerm = _domainGlossaryBuilder.ToImmutable();
        
        TermById = IdAndUiByTerm.ToDictionary(
            kvp => kvp.Value.callbackId,
            kvp => kvp.Key);
    }

    // ToDo: if this all works now then improve AddTerm so up there I don't need Dt() and Ui() 
    private void AddTerm(DomainTerm term, string idRaw, UiString uiString) =>
        _domainGlossaryBuilder.Add(term, (new CallbackId(idRaw), uiString));
}
