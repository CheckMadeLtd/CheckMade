using CheckMade.Common.DomainModel.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.DomainModel.Core.LiveEvents;
using CheckMade.Common.DomainModel.Core.Submissions.Concrete;
using CheckMade.Common.DomainModel.Utils;

namespace CheckMade.Common.DomainModel.Core.Submissions;

public interface ISubmission
{
    Guid Id { get; }
    DateTimeOffset CreationDate { get; }
    ISphereOfAction Sphere { get; }
    Role ReportedBy { get; }
    
    IDomainGlossary Glossary { get; }

    IReadOnlyDictionary<SubmissionSummaryCategories, UiString> GetSummary();
}
