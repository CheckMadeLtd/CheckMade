using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;
using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete.RoleTypes;
using CheckMade.Common.Model.Core.LiveEvents.Concrete.SphereOfActionDetails.Facilities;
using CheckMade.Common.Model.Core.Trades.Concrete.TradeModels.SaniClean;
using CheckMade.Common.Model.Core.Trades.Concrete.TradeModels.SaniClean.Issues;
using CheckMade.Common.Model.Core.Trades.Concrete.TradeModels.SiteClean;
using CheckMade.Common.Model.Core.Trades.Concrete.TradeModels.SiteClean.Issues;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic;

public sealed record DomainGlossary : IDomainGlossary
{
    private readonly ImmutableDictionary<DomainTerm, (CallbackId callbackId, UiString uiString)>.Builder
        _domainGlossaryBuilder =
            ImmutableDictionary.CreateBuilder<DomainTerm, (CallbackId callbackId, UiString uiString)>();
    
    public IReadOnlyDictionary<DomainTerm, (CallbackId callbackId, UiString uiString)> IdAndUiByTerm { get; }
    public IDictionary<CallbackId, DomainTerm> TermById { get; }
    
    public static readonly UiString ToggleOffSuffix = UiNoTranslate("[  ]");
    public static readonly UiString ToggleOnSuffix = UiNoTranslate("[✔]");
    
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
        
        AddTerm(typeof(IUserAuthWorkflow), "DJIQPO", UiNoTranslate(nameof(UserAuthWorkflow)));
        AddTerm(UserAuthWorkflow.States.Initial, "DTWLPM", 
            UiNoTranslate(UserAuthWorkflow.States.Initial.ToString()));
        AddTerm(UserAuthWorkflow.States.ReceivedTokenSubmissionAttempt, "DRGLYG", 
            UiNoTranslate(UserAuthWorkflow.States.ReceivedTokenSubmissionAttempt.ToString()));
        
        AddTerm(typeof(ILanguageSettingWorkflow), "DDI3H3", UiNoTranslate(nameof(LanguageSettingWorkflow)));
        AddTerm(LanguageSettingWorkflow.States.Initial, "DD4252",
            UiNoTranslate(LanguageSettingWorkflow.States.Initial.ToString()));
        AddTerm(LanguageSettingWorkflow.States.ReceivedLanguageSetting, "DGWJX8",
            UiNoTranslate(LanguageSettingWorkflow.States.ReceivedLanguageSetting.ToString()));
        AddTerm(LanguageSettingWorkflow.States.Completed, "DL32QX",
            UiNoTranslate(LanguageSettingWorkflow.States.Completed.ToString()));
        
        AddTerm(typeof(ILogoutWorkflow), "DPAWEY", UiNoTranslate(nameof(LogoutWorkflow)));
        AddTerm(LogoutWorkflow.States.Initial, "DZF3Z4", 
            UiNoTranslate(LogoutWorkflow.States.Initial.ToString()));
        AddTerm(LogoutWorkflow.States.LogoutConfirmed, "DPXFZ8", 
            UiNoTranslate(LogoutWorkflow.States.LogoutConfirmed.ToString()));
        AddTerm(LogoutWorkflow.States.LogoutAborted, "D1T2AR", 
            UiNoTranslate(LogoutWorkflow.States.LogoutAborted.ToString()));
        
        AddTerm(typeof(INewIssueWorkflow), "D6SORL", 
            UiNoTranslate(nameof(NewIssueWorkflow)));
        AddTerm(typeof(INewIssueTradeSelection), "DA0ZMD", 
            UiNoTranslate(nameof(NewIssueTradeSelection)));
        AddTerm(typeof(INewIssueSphereSelection<SaniCleanTrade>), "D8T63V",
            UiNoTranslate(nameof(NewIssueSphereSelection<SaniCleanTrade>)));
        AddTerm(typeof(INewIssueSphereSelection<SiteCleanTrade>), "DYRNZL",
            UiNoTranslate(nameof(NewIssueSphereSelection<SiteCleanTrade>)));
        AddTerm(typeof(INewIssueSphereConfirmation<SaniCleanTrade>), "D45JQ1",
            UiNoTranslate(nameof(NewIssueSphereConfirmation<SaniCleanTrade>)));
        AddTerm(typeof(INewIssueSphereConfirmation<SiteCleanTrade>), "DI6GGV",
            UiNoTranslate(nameof(NewIssueSphereConfirmation<SiteCleanTrade>)));
        AddTerm(typeof(INewIssueTypeSelection<SaniCleanTrade>), "DDQHWW",
            UiNoTranslate(nameof(NewIssueTypeSelection<SaniCleanTrade>)));
        AddTerm(typeof(INewIssueTypeSelection<SiteCleanTrade>), "D88CK2",
            UiNoTranslate(nameof(NewIssueTypeSelection<SiteCleanTrade>)));
        AddTerm(typeof(INewIssueConsumablesSelection<SaniCleanTrade>), "DWBYSV",
            UiNoTranslate(nameof(NewIssueConsumablesSelection<SaniCleanTrade>)));
        AddTerm(typeof(INewIssueEvidenceEntry<SaniCleanTrade>), "DKUR0Z",
            UiNoTranslate(nameof(NewIssueEvidenceEntry<SaniCleanTrade>)));
        AddTerm(typeof(INewIssueEvidenceEntry<SiteCleanTrade>), "DJSD44",
            UiNoTranslate(nameof(NewIssueEvidenceEntry<SiteCleanTrade>)));
        AddTerm(typeof(INewIssueFacilitySelection<SaniCleanTrade>), "DWIY4L",
            UiNoTranslate(nameof(NewIssueFacilitySelection<SaniCleanTrade>)));
        AddTerm(typeof(INewIssueFacilitySelection<SiteCleanTrade>), "D5W0J7",
            UiNoTranslate(nameof(NewIssueFacilitySelection<SiteCleanTrade>)));
        AddTerm(typeof(INewIssueReview<SaniCleanTrade>), "DAH8TX",
            UiNoTranslate(nameof(NewIssueReview<SaniCleanTrade>)));
        AddTerm(typeof(INewIssueReview<SiteCleanTrade>), "DWNXLY",
            UiNoTranslate(nameof(NewIssueReview<SiteCleanTrade>)));
        
        #endregion
        
        #region TradeModelSaniClean

        AddTerm(typeof(CleanlinessIssue), "DAWYZP", Ui("🪣 Cleanliness"));
        AddTerm(typeof(TechnicalIssue), "DM46NG", Ui("🔧 Technical"));
        AddTerm(typeof(ConsumablesIssue), "D582QJ", Ui("🗄 Consumables"));
        AddTerm(typeof(StaffIssue), "D9MRJ9", Ui("🙋 Staff"));

        AddTerm(ConsumablesIssue.Item.ToiletPaper, "DSTP1N", Ui("🧻 Toilet Paper"));
        AddTerm(ConsumablesIssue.Item.PaperTowels, "DOJH85", Ui("🌫️ Paper Towels"));
        AddTerm(ConsumablesIssue.Item.Soap, "D79AMO", Ui("🧴 Soap"));

        AddTerm(typeof(Toilet), "D1540N", Ui("🚽 Toilet"));
        AddTerm(typeof(Shower), "D4W2GW", Ui("🚿 Shower"));
        AddTerm(typeof(GeneralMisc), "D55BLT", Ui("General/Misc"));
        
        AddTerm(typeof(TradeAdmin<SaniCleanTrade>), "DLE960", Ui("SaniClean-Admin"));
        AddTerm(typeof(TradeInspector<SaniCleanTrade>), "DYHG6E", Ui("SaniClean-Inspector"));
        AddTerm(typeof(TradeEngineer<SaniCleanTrade>), "D2PC58", Ui("SaniClean-Engineer"));
        AddTerm(typeof(TradeTeamLead<SaniCleanTrade>), "DE4E59", Ui("SaniClean-CleanLead"));
        AddTerm(typeof(TradeObserver<SaniCleanTrade>), "DH4QH5", Ui("SaniClean-Observer"));
        
        #endregion
        
        #region TradeModelSiteClean
        
        AddTerm(typeof(GeneralIssue), "DVGI3N", Ui("General"));
        
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

    public IReadOnlyCollection<DomainTerm> GetAll(Type superType)
    {
        return
            IdAndUiByTerm
                .Select(kvp => kvp.Key)
                .Where(dt => dt.TypeValue != null &&
                             superType.IsAssignableFrom(dt.TypeValue) ||
                             superType == dt.EnumType)
                .OrderBy(dt => dt.ToString())
                .ToImmutableReadOnlyCollection();
    }

    public string GetId(Type dtType) => IdAndUiByTerm[Dt(dtType)].callbackId;
    public string GetId(Enum dtEnum) => IdAndUiByTerm[Dt(dtEnum)].callbackId;
    public string GetId(DomainTerm domainTerm) => IdAndUiByTerm[domainTerm].callbackId;

    public UiString GetUi(Type dtType) => IdAndUiByTerm[Dt(dtType)].uiString;
    public UiString GetUi(Enum dtEnum) => IdAndUiByTerm[Dt(dtEnum)].uiString;
    public UiString GetUi(DomainTerm domainTerm) => IdAndUiByTerm[domainTerm].uiString;
    
    public Type GetDtType(string dtId)
    {
        var dtType = TermById[new CallbackId(dtId)].TypeValue;

        return 
            dtType ?? 
            throw new ArgumentException(
                $"Given {nameof(dtId)} '{dtId}' not found in {nameof(DomainGlossary)}", 
                nameof(dtId));
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
