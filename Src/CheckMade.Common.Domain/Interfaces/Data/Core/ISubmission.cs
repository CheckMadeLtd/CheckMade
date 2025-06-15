using CheckMade.Common.Domain.Data.Core.Actors.RoleSystem;
using CheckMade.Common.Domain.Data.Core.Submissions;
using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Common.Utils.UiTranslation;

namespace CheckMade.Common.Domain.Interfaces.Data.Core;

public interface ISubmission
{
    Guid Id { get; }
    DateTimeOffset CreationDate { get; }
    ISphereOfAction Sphere { get; }
    Role ReportedBy { get; }
    
    IDomainGlossary Glossary { get; }

    IReadOnlyDictionary<SubmissionSummaryCategories, UiString> GetSummary();
}
