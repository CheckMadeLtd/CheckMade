using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete.RoleTypes;
using CheckMade.Common.Model.Core.Trades.Concrete.SubDomains.SaniClean.Facilities;
using CheckMade.Common.Model.Core.Trades.Concrete.SubDomains.SaniClean.Issues;
using CheckMade.Common.Model.Core.Trades.Concrete.Types;
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
        AddTerm(typeof(LiveEventAdmin), "DD6I1A", Ui("LiveEvent-Admin"));
        AddTerm(typeof(LiveEventObserver), "D5Q5V2", Ui("LiveEvent-Observer"));
        
        AddTerm(LanguageCode.en, "DCQ4ME", Ui("🇬🇧 English"));
        AddTerm(LanguageCode.de, "DFVN7W", Ui("🇩🇪 German"));

        #region Trades
        
        AddTerm(typeof(SaniCleanTrade), "DX3KFI", Ui("🪠 Sanitary Cleaning"));
        AddTerm(typeof(SiteCleanTrade), "DSIL7M", Ui("🧹 Site Cleaning"));
        
        #endregion
        
        #region Workflows
        
        AddTerm(typeof(UserAuthWorkflow), "DJIQPO", UiNoTranslate(nameof(UserAuthWorkflow)));
        AddTerm(typeof(LanguageSettingWorkflow), "DDI3H3", UiNoTranslate(nameof(LanguageSettingWorkflow)));
        AddTerm(typeof(LogoutWorkflow), "DPAWEY", UiNoTranslate(nameof(LogoutWorkflow)));
        AddTerm(typeof(NewIssueWorkflow), "D6SORL", UiNoTranslate(nameof(NewIssueWorkflow)));
        
        #endregion
        
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
        
        AddTerm(typeof(TradeAdmin<SaniCleanTrade>), "DLE960", Ui("SaniClean-Admin"));
        AddTerm(typeof(TradeInspector<SaniCleanTrade>), "DYHG6E", Ui("SaniClean-Inspector"));
        AddTerm(typeof(TradeEngineer<SaniCleanTrade>), "D2PC58", Ui("SaniClean-Engineer"));
        AddTerm(typeof(TradeTeamLead<SaniCleanTrade>), "DE4E59", Ui("SaniClean-CleanLead"));
        AddTerm(typeof(TradeObserver<SaniCleanTrade>), "DH4QH5", Ui("SaniClean-Observer"));
        
        #endregion
        
        #region SubDomainSiteClean
        
        AddTerm(typeof(TradeAdmin<SiteCleanTrade>), "DIV8LK", Ui("SiteClean-Admin"));
        AddTerm(typeof(TradeInspector<SiteCleanTrade>), "DBN6SZ", Ui("SiteClean-Inspector"));
        AddTerm(typeof(TradeEngineer<SiteCleanTrade>), "DWWD3W", Ui("SiteClean-Engineer"));
        AddTerm(typeof(TradeTeamLead<SiteCleanTrade>), "DFIY82", Ui("SiteClean-CleanLead"));
        AddTerm(typeof(TradeObserver<SiteCleanTrade>), "DOV3F2", Ui("SiteClean-Observer"));
        
        #endregion
        
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
