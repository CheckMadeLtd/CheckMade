using CheckMade.Common.DomainModel.Core.Actors.RoleSystem;
using CheckMade.Common.DomainModel.Core.Submissions;
using CheckMade.Common.DomainModel.Interfaces.ChatBotLogic;

namespace CheckMade.Common.DomainModel.Interfaces.Core;

public interface ISubmission
{
    Guid Id { get; }
    DateTimeOffset CreationDate { get; }
    ISphereOfAction Sphere { get; }
    Role ReportedBy { get; }
    
    IDomainGlossary Glossary { get; }

    IReadOnlyDictionary<SubmissionSummaryCategories, UiString> GetSummary();
}
