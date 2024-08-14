using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.LanguageSetting;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.LanguageSetting.States;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.Logout;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.Logout.States;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.UserAuth;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.UserAuth.States;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.A_Init;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.B_Details;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.C_Review;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.D_Terminators;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Reactive;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Reactive.Notifications;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete.RoleTypes;
using CheckMade.Common.Model.Core.LiveEvents.Concrete.SphereOfActionDetails;
using CheckMade.Common.Model.Core.LiveEvents.Concrete.SphereOfActionDetails.Facilities;
using CheckMade.Common.Model.Core.Submissions.Issues.Concrete;
using CheckMade.Common.Model.Core.Submissions.Issues.Concrete.IssueTypes;
using CheckMade.Common.Model.Core.Trades.Concrete;
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
    public static readonly UiString ToggleOnSuffix = UiNoTranslate("[☑️]");
    
    public DomainGlossary()
    {
        AddTerm(LanguageCode.en, "DCQ4ME", Ui("🇬🇧 English"));
        AddTerm(LanguageCode.de, "DFVN7W", Ui("🇩🇪 German"));

        #region Trades
        
        AddTerm(typeof(SaniCleanTrade), "DX3KFI", Ui("🪠 Sanitary Cleaning"));
        AddTerm(typeof(SiteCleanTrade), "DSIL7M", Ui("🧹 Site Cleaning"));
        
        #endregion
        
        #region Workflows
        
        AddTerm(typeof(UserAuthWorkflow), "DJIQPO");
        AddTerm(typeof(IUserAuthWorkflowTokenEntry), "DTWLPM");
        AddTerm(typeof(IUserAuthWorkflowAuthenticated), "DRGLYG");
        
        AddTerm(typeof(LanguageSettingWorkflow), "DDI3H3");
        AddTerm(typeof(ILanguageSettingSelect), "DD4252");
        AddTerm(typeof(ILanguageSettingSet), "DL32QX");
        
        AddTerm(typeof(LogoutWorkflow), "DPAWEY");
        AddTerm(typeof(ILogoutWorkflowConfirm), "DZF3Z4");
        AddTerm(typeof(ILogoutWorkflowLoggedOut), "DPXFZ8");
        AddTerm(typeof(ILogoutWorkflowAborted), "D1T2AR");
        
        AddTerm(typeof(ViewAttachmentsWorkflow), "D3TC83");
        AddTerm(typeof(IOneStepWorkflowTerminator), "DT8SQ7");
        
        AddTerm(typeof(NewIssueWorkflow), "D6SORL");
        AddTerm(typeof(INewIssueTradeSelection), "DA0ZMD");
        
        AddTerm(typeof(INewIssueSphereSelection<SaniCleanTrade>), "D8T63V");
        AddTerm(typeof(INewIssueSphereSelection<SiteCleanTrade>), "DYRNZL");
        
        AddTerm(typeof(INewIssueSphereConfirmation<SaniCleanTrade>), "D45JQ1");
        AddTerm(typeof(INewIssueSphereConfirmation<SiteCleanTrade>), "DI6GGV");
        
        AddTerm(typeof(INewIssueTypeSelection<SaniCleanTrade>), "DDQHWW");
        AddTerm(typeof(INewIssueTypeSelection<SiteCleanTrade>), "D88CK2");
        
        AddTerm(typeof(INewIssueConsumablesSelection<SaniCleanTrade>), "DWBYSV");
        
        AddTerm(typeof(INewIssueEvidenceEntry<SaniCleanTrade>), "DKUR0Z");
        AddTerm(typeof(INewIssueEvidenceEntry<SiteCleanTrade>), "DJSD44");
        
        AddTerm(typeof(INewIssueFacilitySelection<SaniCleanTrade>), "DWIY4L");
        AddTerm(typeof(INewIssueFacilitySelection<SiteCleanTrade>), "D5W0J7");
        
        AddTerm(typeof(INewIssueReview<SaniCleanTrade>), "DAH8TX");
        AddTerm(typeof(INewIssueReview<SiteCleanTrade>), "DWNXLY");
        
        AddTerm(typeof(INewIssueSubmissionSucceeded<SaniCleanTrade>), "D8TGOV");
        AddTerm(typeof(INewIssueSubmissionSucceeded<SiteCleanTrade>), "DM3PCW");
        
        AddTerm(typeof(INewIssueEditMenu<SaniCleanTrade>), "D8ABBA");
        AddTerm(typeof(INewIssueEditMenu<SiteCleanTrade>), "DHZY2B");
        
        AddTerm(typeof(INewIssueCancelConfirmation<SaniCleanTrade>), "DL69OL");
        AddTerm(typeof(INewIssueCancelConfirmation<SiteCleanTrade>), "DNLJMN");
        
        AddTerm(typeof(INewIssueCancelled<SaniCleanTrade>), "DN1KAK");
        AddTerm(typeof(INewIssueCancelled<SiteCleanTrade>), "DR8REC");
        
        #endregion
        
        #region Issues
        
        // Below, presence/absence determines availability of IssueTypes per TradeType, also in the Workflow!

        AddTerm(typeof(GeneralIssue<SaniCleanTrade>), "DVGI3N", Ui("❗ General"));
        AddTerm(typeof(GeneralIssue<SiteCleanTrade>), "D4QM7Q", Ui("❗ General"));
        
        AddTerm(typeof(CleanlinessIssue<SaniCleanTrade>), "DAWYZP", Ui("🪣 Cleanliness"));
        AddTerm(typeof(CleanlinessIssue<SiteCleanTrade>), "DTG4C8", Ui("🪣 Cleanliness"));
        
        AddTerm(typeof(TechnicalIssue<SaniCleanTrade>), "DM46NG", Ui("🔧 Technical"));
        AddTerm(typeof(TechnicalIssue<SiteCleanTrade>), "D4H7RG", Ui("🔧 Technical"));
        
        AddTerm(typeof(ConsumablesIssue<SaniCleanTrade>), "D582QJ", Ui("🗄 Missing Consumables"));
        
        AddTerm(typeof(StaffIssue<SaniCleanTrade>), "D9MRJ9", Ui("🙋 Staff"));
        AddTerm(typeof(StaffIssue<SiteCleanTrade>), "DVVL0F", Ui("🙋 Staff"));

        AddTerm(IssueStatus.Drafting, "DC5E1H", Ui("✏️ Drafting"));
        AddTerm(IssueStatus.Reported, "DNYU8L", Ui("📤️ Reported"));
        AddTerm(IssueStatus.InProgress, "D15NQR", Ui("⏳ In Progress"));
        AddTerm(IssueStatus.ReviewRequired, "DBGOMN", Ui("📋 Review Required"));
        AddTerm(IssueStatus.ReviewNotPassed, "DV6EBL", Ui("❌ Review Not Passed"));
        AddTerm(IssueStatus.Closed, "D2PTQ6", Ui("✅ Closed"));
        
        #endregion
        
        #region Facilities
        
        AddTerm(typeof(Toilet), "D1540N", Ui("🚽 Toilet"));
        AddTerm(typeof(Shower), "D4W2GW", Ui("🚿 Shower"));
        AddTerm(typeof(GeneralMisc), "D55BLT", Ui("General/Misc"));

        AddTerm(ConsumablesItem.ToiletPaper, "DSTP1N", Ui("🧻 Toilet Paper"));
        AddTerm(ConsumablesItem.PaperTowels, "DOJH85", Ui("🌫️ Paper Towels"));
        AddTerm(ConsumablesItem.Soap, "D79AMO", Ui("🧴 Soap"));
        
        #endregion

        #region RoleTypes

        AddTerm(typeof(LiveEventAdmin), "DD6I1A", Ui("LiveEvent-Admin"));
        AddTerm(typeof(LiveEventObserver), "D5Q5V2", Ui("LiveEvent-Observer"));

        AddTerm(typeof(TradeAdmin<SaniCleanTrade>), "DLE960", Ui("SaniClean-Admin"));
        AddTerm(typeof(TradeInspector<SaniCleanTrade>), "DYHG6E", Ui("SaniClean-Inspector"));
        AddTerm(typeof(TradeEngineer<SaniCleanTrade>), "D2PC58", Ui("SaniClean-Engineer"));
        AddTerm(typeof(TradeTeamLead<SaniCleanTrade>), "DE4E59", Ui("SaniClean-CleanLead"));
        AddTerm(typeof(TradeObserver<SaniCleanTrade>), "DH4QH5", Ui("SaniClean-Observer"));
        
        AddTerm(typeof(TradeAdmin<SiteCleanTrade>), "DIV8LK", Ui("SiteClean-Admin"));
        AddTerm(typeof(TradeInspector<SiteCleanTrade>), "DBN6SZ", Ui("SiteClean-Inspector"));
        AddTerm(typeof(TradeEngineer<SiteCleanTrade>), "DWWD3W", Ui("SiteClean-Engineer"));
        AddTerm(typeof(TradeTeamLead<SiteCleanTrade>), "DFIY82", Ui("SiteClean-TeamLead"));
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

    public string GetIdForEquallyNamedInterface(Type dtType) =>
        GetId(dtType
            .GetInterfaces()
            .First(i => i.Name.Contains(dtType.Name)));
    
    public UiString GetUi(Type dtType) => IdAndUiByTerm[Dt(dtType)].uiString;
    public UiString GetUi(Enum dtEnum) => IdAndUiByTerm[Dt(dtEnum)].uiString;
    public UiString GetUi(DomainTerm domainTerm) => IdAndUiByTerm[domainTerm].uiString;
    
    public UiString GetUi(IReadOnlyCollection<Enum> dtEnums) =>
        UiConcatenate(
            dtEnums
                .Select(item => UiConcatenate(
                    GetUi(item), UiNoTranslate("; ")))
                .ToArray());
    
    public UiString GetUi(IReadOnlyCollection<DomainTerm> domainTerms) =>
        UiConcatenate(
            domainTerms
                .Select(item => UiConcatenate(
                    GetUi(item), UiNoTranslate("; ")))
                .ToArray());

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

    private void AddTerm(Type typeTerm, string idRaw) =>
        AddTerm(typeTerm, idRaw, UiNoTranslate(typeTerm.Name));
}
