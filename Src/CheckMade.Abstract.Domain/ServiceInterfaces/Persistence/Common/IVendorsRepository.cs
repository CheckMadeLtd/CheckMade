using CheckMade.Abstract.Domain.Model.Common.Actors;

namespace CheckMade.Abstract.Domain.ServiceInterfaces.Persistence.Common;

public interface IVendorsRepository
{
    Task<Vendor?> GetAsync(string vendorName);
    Task<IReadOnlyCollection<Vendor>> GetAllAsync();
}