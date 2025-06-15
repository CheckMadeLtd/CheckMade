using CheckMade.Common.DomainModel.Data.Core.Actors;

namespace CheckMade.Common.DomainModel.Interfaces.Persistence.Core;

public interface IVendorsRepository
{
    Task<Vendor?> GetAsync(string vendorName);
    Task<IReadOnlyCollection<Vendor>> GetAllAsync();
}