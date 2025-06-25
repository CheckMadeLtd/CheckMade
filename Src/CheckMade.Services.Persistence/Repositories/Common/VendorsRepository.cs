using System.Collections.Concurrent;
using System.Data.Common;
using CheckMade.Core.Model.Common.Actors;
using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Core.ServiceInterfaces.Persistence.Common;
using General.Utils.FpExtensions.Monads;
using static CheckMade.Services.Persistence.Constitutors.StaticConstitutors;

namespace CheckMade.Services.Persistence.Repositories.Common;

public sealed class VendorsRepository(IDbExecutionHelper dbHelper, IDomainGlossary glossary) 
    : BaseRepository(dbHelper, glossary), IVendorsRepository
{
    private readonly ConcurrentDictionary<string, Task<IReadOnlyCollection<Vendor>>> _cache = new();
    private const string CacheKey = "all";

    private static readonly Func<DbDataReader, IDomainGlossary, Vendor> VendorMapper = 
        static (reader, _) => 
            ConstituteVendor(reader).GetValueOrThrow();
    
    public async Task<Vendor?> GetAsync(string vendorName) =>
        (await GetAllAsync())
        .FirstOrDefault(v => v.Name == vendorName);

    public async Task<IReadOnlyCollection<Vendor>> GetAllAsync() =>
        await _cache.GetOrAdd(CacheKey, async _ => await LoadAllFromDbAsync());

    private async Task<IReadOnlyCollection<Vendor>> LoadAllFromDbAsync()
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
        
        return await ExecuteMapperAsync(command, VendorMapper);
    }
}
