using CheckMade.Core.Model.Common.Actors;
using CheckMade.Core.Model.Common.LiveEvents;
using CheckMade.Core.ServiceInterfaces.Bot;
using General.Utils.UiTranslation;

namespace CheckMade.Core.Model.Common.Submissions;

public interface ISubmission
{
    Guid Id { get; }
    DateTimeOffset CreationDate { get; }
    ISphereOfAction Sphere { get; }
    Role ReportedBy { get; }
    
    IDomainGlossary Glossary { get; }

    IReadOnlyDictionary<SubmissionSummaryCategories, UiString> GetSummary();
}
