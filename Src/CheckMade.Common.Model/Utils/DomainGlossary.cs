using System.Collections.Immutable;
using CheckMade.Common.Model.Core.SanitaryOps.Facilities;
using CheckMade.Common.Model.Core.SanitaryOps.Issues;

namespace CheckMade.Common.Model.Utils;

public class DomainGlossary
{
    private readonly ImmutableDictionary<OneOf<int, Type>, (CallbackId callbackId, UiString uiString)>.Builder
        _domainGlossaryBuilder =
            ImmutableDictionary.CreateBuilder<OneOf<int, Type>, (CallbackId callbackId, UiString uiString)>();
    
    public IReadOnlyDictionary<OneOf<int, Type>, (CallbackId callbackId, UiString uiString)> IdAndUiByTerm { get; }

    public IDictionary<CallbackId, OneOf<int, Type>> TermById { get; }
    
    public DomainGlossary()
    {
        AddTerm(typeof(CleanlinessIssue), "DAWYZP", Ui("🪣 Cleanliness"));
        AddTerm(typeof(TechnicalIssue), "DM46NG", Ui("🔧 Technical"));
        AddTerm(typeof(ConsumablesIssue), "D582QJ", Ui("🗄 Consumables"));

        AddTerm((int)ConsumablesIssue.Item.ToiletPaper, "DSTP1N", Ui("🧻 Toilet Paper"));
        AddTerm((int)ConsumablesIssue.Item.PaperTowels, "DOJH85", Ui("🌫️ Paper Towels"));
        AddTerm((int)ConsumablesIssue.Item.Soap, "D79AMO", Ui("🧴 Soap"));

        AddTerm(typeof(Toilet), "D1540N", Ui("🚽 Toilet"));
        AddTerm(typeof(Shower), "D4W2GW", Ui("🚿 Shower"));
        AddTerm(typeof(Staff), "D9MRJ9", Ui("🙋 Staff"));

        IdAndUiByTerm = _domainGlossaryBuilder.ToImmutable();
        
        TermById = IdAndUiByTerm.ToDictionary(
            kvp => kvp.Value.callbackId,
            kvp => kvp.Key);
    }

    private void AddTerm(OneOf<int, Type> term, string idRaw, UiString uiString) =>
        _domainGlossaryBuilder.Add(
            term,
            (new CallbackId(idRaw), uiString));
}
