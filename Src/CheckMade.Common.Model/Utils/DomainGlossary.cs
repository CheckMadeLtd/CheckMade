using System.Collections.Immutable;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Trades.SubDomains.SanitaryOps.Facilities;
using CheckMade.Common.Model.Core.Trades.SubDomains.SanitaryOps.Issues;
using CheckMade.Common.Model.Core.Trades.Types;

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
        #region SubDomainSanitaryOps

        AddTerm(typeof(CleanlinessIssue), "DAWYZP", "🪣 Cleanliness");
        AddTerm(typeof(TechnicalIssue), "DM46NG", "🔧 Technical");
        AddTerm(typeof(ConsumablesIssue), "D582QJ", "🗄 Consumables");

        AddTerm(ConsumablesIssue.Item.ToiletPaper, "DSTP1N", "🧻 Toilet Paper");
        AddTerm(ConsumablesIssue.Item.PaperTowels, "DOJH85", "🌫️ Paper Towels");
        AddTerm(ConsumablesIssue.Item.Soap, "D79AMO", "🧴 Soap");

        AddTerm(typeof(Toilet), "D1540N", "🚽 Toilet");
        AddTerm(typeof(Shower), "D4W2GW", "🚿 Shower");
        AddTerm(typeof(Staff), "D9MRJ9", "🙋 Staff");
        
        // ToDo: add RoleTypes once switched over from Enum to Types (de.tsv already has en/de strings for it!!
        
        #endregion
        
        #region Trades
        
        AddTerm(typeof(TradeSanitaryOps), "DX3KFI", "🪠 Sanitary Operations");
        AddTerm(typeof(TradeSiteCleaning), "DSIL7M", "🧹 Site Cleaning");
        
        #endregion
        
        AddTerm(LanguageCode.en, "DCQ4ME", "🇬🇧 English");
        AddTerm(LanguageCode.de, "DFVN7W", "🇩🇪 German");

        IdAndUiByTerm = _domainGlossaryBuilder.ToImmutable();
        
        TermById = IdAndUiByTerm.ToDictionary(
            kvp => kvp.Value.callbackId,
            kvp => kvp.Key);
    }

    private void AddTerm(object term, string idRaw, string uiString)
    {
        var callBackIdAndUi = (new CallbackId(idRaw), Ui(uiString));

        switch (term)
        {
            case Type termType:
                _domainGlossaryBuilder.Add(Dt(termType), callBackIdAndUi);
                break;
            
            case Enum termEnum:
                _domainGlossaryBuilder.Add(Dt(termEnum), callBackIdAndUi);
                break;
            
            default:
                throw new ArgumentException("Term needs to be either of type Type or of type Enum!");
        }
    }
}
