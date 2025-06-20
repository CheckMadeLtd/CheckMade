using CheckMade.Core.Model.Common.Actors;

namespace CheckMade.Core.ServiceInterfaces.Persistence.Common;

public interface IVendorsRepository
{
    Task<Vendor?> GetAsync(string vendorName);
    Task<IReadOnlyCollection<Vendor>> GetAllAsync();
}