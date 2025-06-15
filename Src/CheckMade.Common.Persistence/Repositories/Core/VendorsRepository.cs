using System.Data.Common;
using CheckMade.Common.DomainModel.Data.Core.Actors;
using CheckMade.Common.DomainModel.Interfaces.ChatBot.Logic;
using CheckMade.Common.DomainModel.Interfaces.Persistence.Core;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using static CheckMade.Common.Persistence.Repositories.DomainModelConstitutors;

namespace CheckMade.Common.Persistence.Repositories.Core;

public sealed class VendorsRepository(IDbExecutionHelper dbHelper, IDomainGlossary glossary) 
    : BaseRepository(dbHelper, glossary), IVendorsRepository
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    
    private Option<IReadOnlyCollection<Vendor>> _cache = Option<IReadOnlyCollection<Vendor>>.None();

    private static readonly Func<DbDataReader, IDomainGlossary, Vendor> VendorMapper = 
        static (reader, _) => 
            ConstituteVendor(reader).GetValueOrThrow();
    
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
                    var vendors = await ExecuteMapperAsync(command, VendorMapper);
                    
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
