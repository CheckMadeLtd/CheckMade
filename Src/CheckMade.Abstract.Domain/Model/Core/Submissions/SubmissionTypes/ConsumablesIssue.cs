using System.Collections.Frozen;
using System.Collections.Immutable;
using CheckMade.Abstract.Domain.Model.Core.Actors;
using CheckMade.Abstract.Domain.Model.Core.LiveEvents;
using CheckMade.Abstract.Domain.Model.Core.LiveEvents.SphereOfActionDetails;
using CheckMade.Abstract.Domain.Model.Core.Trades;
using CheckMade.Abstract.Domain.ServiceInterfaces.Bot;
using General.Utils.UiTranslation;
using static CheckMade.Abstract.Domain.Model.Core.Submissions.SubmissionSummaryCategories;

namespace CheckMade.Abstract.Domain.Model.Core.Submissions.SubmissionTypes;

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