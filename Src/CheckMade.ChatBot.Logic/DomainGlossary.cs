using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Trades.SubDomains.SaniClean.Facilities;
using CheckMade.Common.Model.Core.Trades.SubDomains.SaniClean.Issues;
using CheckMade.Common.Model.Core.Trades.Types;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic;

public class DomainGlossary : IDomainGlossary
{
    private readonly ImmutableDictionary<DomainTerm, (CallbackId callbackId, UiString uiString)>.Builder
        _domainGlossaryBuilder =
            ImmutableDictionary.CreateBuilder<DomainTerm, (CallbackId callbackId, UiString uiString)>();
    
    public IReadOnlyDictionary<DomainTerm, (CallbackId callbackId, UiString uiString)> IdAndUiByTerm { get; }

    public IDictionary<CallbackId, DomainTerm> TermById { get; }
    
    public DomainGlossary()
    {
        #region SubDomainSaniClean

        AddTerm(typeof(CleanlinessIssue), "DAWYZP", Ui("🪣 Cleanliness"));
        AddTerm(typeof(TechnicalIssue), "DM46NG", Ui("🔧 Technical"));
        AddTerm(typeof(InventoryIssue), "D582QJ", Ui("🗄 Consumables"));

        AddTerm(Consumables.Item.ToiletPaper, "DSTP1N", Ui("🧻 Toilet Paper"));
        AddTerm(Consumables.Item.PaperTowels, "DOJH85", Ui("🌫️ Paper Towels"));
        AddTerm(Consumables.Item.Soap, "D79AMO", Ui("🧴 Soap"));

        AddTerm(typeof(Toilet), "D1540N", Ui("🚽 Toilet"));
        AddTerm(typeof(Shower), "D4W2GW", Ui("🚿 Shower"));
        AddTerm(typeof(StaffIssue), "D9MRJ9", Ui("🙋 StaffIssue"));
        
        // AddTerm(typeof(TradeAdmin<TradeSaniClean>), "123456", Ui("SaniCleanAdmin"));
        
        // ToDo: add RoleTypes once switched over from Enum to Types (de.tsv already has en/de strings for it!!
        
        #endregion
        
        #region Trades
        
        AddTerm(typeof(TradeSaniClean), "DX3KFI", Ui("🪠 Sanitary Operations"));
        AddTerm(typeof(TradeSiteClean), "DSIL7M", Ui("🧹 Site Cleaning"));
        
        #endregion
        
        #region Workflows
        
        AddTerm(typeof(UserAuthWorkflow), "DJIQPO", UiNoTranslate(nameof(UserAuthWorkflow)));
        AddTerm(typeof(LanguageSettingWorkflow), "DDI3H3", UiNoTranslate(nameof(LanguageSettingWorkflow)));
        AddTerm(typeof(LogoutWorkflow), "DPAWEY", UiNoTranslate(nameof(LogoutWorkflow)));
        AddTerm(typeof(NewIssueWorkflow), "D6SORL", UiNoTranslate(nameof(NewIssueWorkflow)));
        
        // Spare codes
        // IV8LK
        // BN6SZ
        // WWD3W
        // FIY82
            
        #endregion
        
        AddTerm(LanguageCode.en, "DCQ4ME", Ui("🇬🇧 English"));
        AddTerm(LanguageCode.de, "DFVN7W", Ui("🇩🇪 German"));

        IdAndUiByTerm = _domainGlossaryBuilder.ToImmutable();
        
        TermById = IdAndUiByTerm.ToDictionary(
            kvp => kvp.Value.callbackId,
            kvp => kvp.Key);
    }

    private void AddTerm(object term, string idRaw, UiString uiString)
    {
        var callBackIdAndUi = (new CallbackId(idRaw), uiString);

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
