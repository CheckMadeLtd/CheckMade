using CheckMade.Abstract.Domain.Model.Core.CrossCutting;

namespace CheckMade.Abstract.Domain.Model.Core.LiveEvents.SphereOfActionDetails;

internal static class Validators
{
    internal static void ValidateFacilityDomainTerms(this IReadOnlyCollection<DomainTerm> facilities)
    {
        foreach (var domainTerm in facilities)
        {
            if (!domainTerm.TypeValue!.IsAssignableTo(typeof(IFacility)))
            {
                throw new ArgumentException(
                    $"Expected {nameof(DomainTerm)} of type {nameof(IFacility)} but got " +
                    $"{domainTerm.TypeValue!.Name} instead");
            }
        }
    }

    internal static void ValidateConsumablesDomainTerms(this IReadOnlyCollection<DomainTerm> consumables)
    {
        foreach (var domainTerm in consumables)
        {
            if (domainTerm.EnumType != typeof(ConsumablesItem))
            {
                throw new ArgumentException(
                    $"Expected {nameof(DomainTerm)} of type {nameof(ConsumablesItem)} but got " +
                    $"{domainTerm.EnumType?.Name ?? domainTerm.TypeValue!.Name} instead");
            }
        }
    }
}