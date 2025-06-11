using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.LanguageSetting;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.LanguageSetting.States;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.Logout;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.Logout.States;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.UserAuth;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Global.UserAuth.States;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewSubmission;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewSubmission.States.A_Init;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewSubmission.States.B_Details;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewSubmission.States.C_Review;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewSubmission.States.D_Terminators;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete.RoleTypes;
using CheckMade.Common.Model.Core.LiveEvents.Concrete.SphereOfActionDetails;
using CheckMade.Common.Model.Core.LiveEvents.Concrete.SphereOfActionDetails.Facilities;
using CheckMade.Common.Model.Core.Submissions.Concrete;
using CheckMade.Common.Model.Core.Submissions.Concrete.SubmissionTypes;
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
        
        AddTerm(typeof(SanitaryTrade), "DX3KFI", Ui("🪠 Sanitary"));
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
        
        AddTerm(typeof(NewIssueWorkflow), "D6SORL");
        AddTerm(typeof(INewSubmissionTradeSelection), "DA0ZMD");
        
        AddTerm(typeof(INewSubmissionSphereSelection<SanitaryTrade>), "D8T63V");
        AddTerm(typeof(INewSubmissionSphereSelection<SiteCleanTrade>), "DYRNZL");
        
        AddTerm(typeof(INewSubmissionSphereConfirmation<SanitaryTrade>), "D45JQ1");
        AddTerm(typeof(INewSubmissionSphereConfirmation<SiteCleanTrade>), "DI6GGV");
        
        AddTerm(typeof(INewSubmissionTypeSelection<SanitaryTrade>), "DDQHWW");
        AddTerm(typeof(INewSubmissionTypeSelection<SiteCleanTrade>), "D88CK2");
        
        AddTerm(typeof(INewSubmissionConsumablesSelection<SanitaryTrade>), "DWBYSV");
        
        AddTerm(typeof(INewSubmissionEvidenceEntry<SanitaryTrade>), "DKUR0Z");
        AddTerm(typeof(INewSubmissionEvidenceEntry<SiteCleanTrade>), "DJSD44");
        
        AddTerm(typeof(INewSubmissionFacilitySelection<SanitaryTrade>), "DWIY4L");
        AddTerm(typeof(INewSubmissionFacilitySelection<SiteCleanTrade>), "D5W0J7");
        
        AddTerm(typeof(INewSubmissionAssessmentRating<SanitaryTrade>), "D1K6AS");
        AddTerm(typeof(INewSubmissionAssessmentRating<SiteCleanTrade>), "DCQGAL");

        AddTerm(typeof(INewSubmissionReview<SanitaryTrade>), "DAH8TX");
        AddTerm(typeof(INewSubmissionReview<SiteCleanTrade>), "DWNXLY");
        
        AddTerm(typeof(INewIssueSubmissionSucceeded<SanitaryTrade>), "D8TGOV");
        AddTerm(typeof(INewIssueSubmissionSucceeded<SiteCleanTrade>), "DM3PCW");
        
        AddTerm(typeof(INewSubmissionEditMenu<SanitaryTrade>), "D8ABBA");
        AddTerm(typeof(INewSubmissionEditMenu<SiteCleanTrade>), "DHZY2B");
        
        AddTerm(typeof(INewSubmissionCancelConfirmation<SanitaryTrade>), "DL69OL");
        AddTerm(typeof(INewSubmissionCancelConfirmation<SiteCleanTrade>), "DNLJMN");
        
        AddTerm(typeof(INewIssueCancelled<SanitaryTrade>), "DN1KAK");
        AddTerm(typeof(INewIssueCancelled<SiteCleanTrade>), "DR8REC");

        #endregion
        
        #region Submissions
        
        // Below, presence/absence determines the availability of SubmissionTypes per TradeType, also in the Workflow!

        AddTerm(typeof(GeneralIssue<SanitaryTrade>), "DVGI3N", Ui("❗ General"));
        AddTerm(typeof(GeneralIssue<SiteCleanTrade>), "D4QM7Q", Ui("❗ General"));
        
        AddTerm(typeof(CleaningIssue<SanitaryTrade>), "DAWYZP", Ui("🪣 Cleaning Issue"));
        AddTerm(typeof(CleaningIssue<SiteCleanTrade>), "DTG4C8", Ui("🪣 Cleaning Issue"));
        
        AddTerm(typeof(TechnicalIssue<SanitaryTrade>), "DM46NG", Ui("🔧 Technical Issue"));
        AddTerm(typeof(TechnicalIssue<SiteCleanTrade>), "D4H7RG", Ui("🔧 Technical Issue"));
        
        AddTerm(typeof(ConsumablesIssue<SanitaryTrade>), "D582QJ", Ui("🗄 Missing Consumables"));
        
        AddTerm(typeof(StaffIssue<SanitaryTrade>), "D9MRJ9", Ui("🙋 Staff"));
        AddTerm(typeof(StaffIssue<SiteCleanTrade>), "DVVL0F", Ui("🙋 Staff"));
        
        AddTerm(typeof(Assessment<SanitaryTrade>), "D440IA", Ui("📋 Cleaning Assessment"));
        AddTerm(typeof(Assessment<SiteCleanTrade>), "DJOMJN", Ui("📋 Cleaning Assessment"));
        
        AddTerm(AssessmentRating.Good, "DYOY4X", Ui("(1) 👍 Good"));
        AddTerm(AssessmentRating.Ok, "D8WD05", Ui("(2) 😐 Not Good"));
        AddTerm(AssessmentRating.Bad, "DGUVKZ", Ui("(3) 👎 Disastrous"));
        
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

        AddTerm(typeof(TradeAdmin<SanitaryTrade>), "DLE960", Ui("Sanitary-Admin"));
        AddTerm(typeof(TradeInspector<SanitaryTrade>), "DYHG6E", Ui("Sanitary-Inspector"));
        AddTerm(typeof(TradeEngineer<SanitaryTrade>), "D2PC58", Ui("Sanitary-Engineer"));
        AddTerm(typeof(TradeTeamLead<SanitaryTrade>), "DE4E59", Ui("Sanitary-CleanLead"));
        AddTerm(typeof(TradeObserver<SanitaryTrade>), "DH4QH5", Ui("Sanitary-Observer"));
        
        AddTerm(typeof(TradeAdmin<SiteCleanTrade>), "DIV8LK", Ui("SiteClean-Admin"));
        AddTerm(typeof(TradeInspector<SiteCleanTrade>), "DBN6SZ", Ui("SiteClean-Inspector"));
        AddTerm(typeof(TradeEngineer<SiteCleanTrade>), "DWWD3W", Ui("SiteClean-Engineer"));
        AddTerm(typeof(TradeTeamLead<SiteCleanTrade>), "DFIY82", Ui("SiteClean-TeamLead"));
        AddTerm(typeof(TradeObserver<SiteCleanTrade>), "DOV3F2", Ui("SiteClean-Observer"));
        
        #endregion
        
        IdAndUiByTerm = _domainGlossaryBuilder.ToImmutable();
        
        TermById = IdAndUiByTerm.ToDictionary(
            static kvp => kvp.Value.callbackId,
            static kvp => kvp.Key);
    }

    public IReadOnlyCollection<DomainTerm> GetAll(Type superType)
    {
        return
            IdAndUiByTerm
                .Select(static kvp => kvp.Key)
                .Where(dt => dt.TypeValue != null &&
                             superType.IsAssignableFrom(dt.TypeValue) ||
                             superType == dt.EnumType)
                .OrderBy(static dt => dt.ToString())
                .ToImmutableArray();
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
