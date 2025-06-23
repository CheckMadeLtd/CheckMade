using System.Collections.Immutable;
using System.Data.Common;
using CheckMade.Core.Model.Common.Actors;
using CheckMade.Core.Model.Common.LiveEvents;
using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Services.Persistence.Constitutors;
using static CheckMade.Services.Persistence.Constitutors.StaticConstitutors;

namespace CheckMade.Services.Persistence.Repositories.Common;

public sealed class RolesSharedMapper(SphereOfActionDetailsConstitutor constitutors)
{
    private static readonly Func<DbDataReader, int> GetRoleKey = 
        static reader => reader.GetInt32(reader.GetOrdinal("role_id"));

    internal Func<DbDataReader, IDomainGlossary, Role> CreateRoleWithoutSphereAssignments { get; } =
        static (reader, glossary) =>
            new Role(
                ConstituteRoleInfo(reader, glossary).GetValueOrThrow(),
                ConstituteUserInfo(reader),
                ConstituteLiveEventInfo(reader).GetValueOrThrow(),
                new HashSet<ISphereOfAction>()
            );

    internal Action<Role, DbDataReader> GetAccumulateSphereAssignments(IDomainGlossary glossary) =>
        (role, reader) =>
        {
            var assignedSphere = constitutors.ConstituteSphereOfAction(reader, glossary);
            if (assignedSphere.IsSome)
                ((HashSet<ISphereOfAction>)role.AssignedToSpheres).Add(assignedSphere.GetValueOrThrow());
        };

    internal Func<Role, Role> FinalizeSphereAssignments { get; } = 
        static role => role with
        {
            AssignedToSpheres = role.AssignedToSpheres.ToImmutableArray()
        };

    internal (Func<DbDataReader, int> keyGetter,
        Func<DbDataReader, Role> modelInitializer,
        Action<Role, DbDataReader> accumulateData,
        Func<Role, Role> modelFinalizer)
        RoleMapper(IDomainGlossary glossary)
    {
        return (
            keyGetter: GetRoleKey,
            modelInitializer: reader => CreateRoleWithoutSphereAssignments(reader, glossary),
            accumulateData: GetAccumulateSphereAssignments(glossary),
            modelFinalizer: FinalizeSphereAssignments
        );
    }
}