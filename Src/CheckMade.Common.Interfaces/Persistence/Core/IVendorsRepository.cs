using CheckMade.Common.Model.Core.Actors.Concrete;

namespace CheckMade.Common.Interfaces.Persistence.Core;

public interface IVendorsRepository
{
    Task<Vendor?> GetAsync(string vendorName);
    Task<IReadOnlyCollection<Vendor>> GetAllAsync();
}