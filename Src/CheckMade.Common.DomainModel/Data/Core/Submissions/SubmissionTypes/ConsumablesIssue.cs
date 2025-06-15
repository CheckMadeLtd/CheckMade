using System.Collections.Frozen;
using System.Collections.Immutable;
using CheckMade.Common.DomainModel.Data.Core.Actors.RoleSystem;
using CheckMade.Common.DomainModel.Data.Core.LiveEvents.SphereOfActionDetails;
using CheckMade.Common.DomainModel.Interfaces.ChatBot.Logic;
using CheckMade.Common.DomainModel.Interfaces.Data.Core;
using static CheckMade.Common.DomainModel.Data.Core.Submissions.SubmissionSummaryCategories;

namespace CheckMade.Common.DomainModel.Data.Core.Submissions.SubmissionTypes;

public sealed record ConsumablesIssue<T>(
    Guid Id,
    DateTimeOffset CreationDate,
    ISphereOfAction Sphere,
    IReadOnlyCollection<ConsumablesItem> AffectedItems,
    Role ReportedBy,
    IDomainGlossary Glossary) 
    : ITradeSubmission<T> where T : ITrade, new()
{
    public IReadOnlyDictionary<SubmissionSummaryCategories, UiString> GetSummary()
    {
        return new Dictionary<SubmissionSummaryCategories, UiString>
        {
            [CommonBasics] = SubmissionFormatters.FormatCommonBasics(this, Glossary),
            [OperationalInfo] = SubmissionFormatters.FormatOperationalInfo(this, Glossary),
            [SubmissionTypeSpecificInfo] = FormatConsumableItems()
        }.ToFrozenDictionary();
    }

    private UiString FormatConsumableItems()
    {
        return UiConcatenate(
            Ui("<b>Affected consumables:</b> "),
            Glossary.GetUi(AffectedItems
                .Select(static ai => (Enum)ai)
                .ToImmutableArray()));
    }
}