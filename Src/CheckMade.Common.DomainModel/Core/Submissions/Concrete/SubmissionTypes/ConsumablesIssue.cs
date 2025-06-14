using System.Collections.Frozen;
using System.Collections.Immutable;
using CheckMade.Common.DomainModel.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.DomainModel.Core.LiveEvents;
using CheckMade.Common.DomainModel.Core.LiveEvents.Concrete.SphereOfActionDetails;
using CheckMade.Common.DomainModel.Core.Trades;
using CheckMade.Common.DomainModel.Utils;
using static CheckMade.Common.DomainModel.Core.Submissions.Concrete.SubmissionSummaryCategories;

namespace CheckMade.Common.DomainModel.Core.Submissions.Concrete.SubmissionTypes;

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