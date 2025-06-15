using CheckMade.Common.Domain.Data.Core.Actors;

namespace CheckMade.Common.Domain.Interfaces.Persistence.Core;

public interface IVendorsRepository
{
    Task<Vendor?> GetAsync(string vendorName);
    Task<IReadOnlyCollection<Vendor>> GetAllAsync();
}