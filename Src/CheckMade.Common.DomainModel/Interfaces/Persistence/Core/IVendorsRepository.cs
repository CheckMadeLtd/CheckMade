using CheckMade.Common.Model.Core.Actors.Concrete;

namespace CheckMade.Common.DomainModel.Interfaces.Persistence.Core;

public interface IVendorsRepository
{
    Task<Vendor?> GetAsync(string vendorName);
    Task<IReadOnlyCollection<Vendor>> GetAllAsync();
}