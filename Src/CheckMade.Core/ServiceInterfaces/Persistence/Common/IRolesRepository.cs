using System.Data.Common;
using CheckMade.Core.Model.Common.Actors;
using CheckMade.Core.ServiceInterfaces.Bot;

namespace CheckMade.Core.ServiceInterfaces.Persistence.Common;

public interface IRolesRepository
{
    Task<Role?> GetAsync(IRoleInfo role);
    Task<IReadOnlyCollection<Role>> GetAllAsync();
    
    Func<DbDataReader, IDomainGlossary, Role> CreateRoleWithoutSphereAssignments { get; }
    Action<Role, DbDataReader> GetAccumulateSphereAssignments(IDomainGlossary glossary);
    Func<Role, Role> FinalizeSphereAssignments { get; }
}