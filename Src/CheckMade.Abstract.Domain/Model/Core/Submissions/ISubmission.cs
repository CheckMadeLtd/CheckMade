using CheckMade.Abstract.Domain.Model.Core.Actors;
using CheckMade.Abstract.Domain.Model.Core.LiveEvents;
using CheckMade.Abstract.Domain.ServiceInterfaces.Bot;
using General.Utils.UiTranslation;

namespace CheckMade.Abstract.Domain.Model.Core.Submissions;

public interface ISubmission
{
    Guid Id { get; }
    DateTimeOffset CreationDate { get; }
    ISphereOfAction Sphere { get; }
    Role ReportedBy { get; }
    
    IDomainGlossary Glossary { get; }

    IReadOnlyDictionary<SubmissionSummaryCategories, UiString> GetSummary();
}
