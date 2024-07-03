using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.Core.Actors;

namespace CheckMade.Common.Persistence.Repositories.Core;

public class VendorsRepository(IDbExecutionHelper dbHelper) 
    : BaseRepository(dbHelper), IVendorsRepository
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    
    private Option<IReadOnlyCollection<Vendor>> _cache = Option<IReadOnlyCollection<Vendor>>.None();

    public async Task<Vendor?> GetAsync(string vendorName) =>
        (await GetAllAsync())
        .FirstOrDefault(v => v.Name == vendorName);

    public async Task<IReadOnlyCollection<Vendor>> GetAllAsync()
    {
        if (_cache.IsNone)
        {
            await Semaphore.WaitAsync();

            try
            {
                if (_cache.IsNone)
                {
                    const string rawQuery = """
                                            SELECT 
                                            
                                            v.name AS vendor_name,
                                            v.status AS vendor_status,
                                            v.details AS vendor_details
                                            
                                            FROM vendors v 
                                            ORDER BY id
                                            """;

                    var command = GenerateCommand(rawQuery, Option<Dictionary<string, object>>.None());
                    
                    var vendors = 
                        await ExecuteReaderOneToOneAsync(command, ModelReaders.ReadVendor);
                    
                    _cache = Option<IReadOnlyCollection<Vendor>>.Some(vendors);
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }

        return _cache.GetValueOrThrow();
    }
}
