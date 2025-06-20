using CheckMade.Abstract.Domain.Model.Core.Actors;

namespace CheckMade.Abstract.Domain.ServiceInterfaces.Persistence.Core;

public interface IVendorsRepository
{
    Task<Vendor?> GetAsync(string vendorName);
    Task<IReadOnlyCollection<Vendor>> GetAllAsync();
}