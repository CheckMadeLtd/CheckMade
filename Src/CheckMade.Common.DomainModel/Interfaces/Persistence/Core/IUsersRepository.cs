using CheckMade.Common.DomainModel.Core;
using CheckMade.Common.DomainModel.Core.Actors;
using CheckMade.Common.DomainModel.Interfaces.Core;

namespace CheckMade.Common.DomainModel.Interfaces.Persistence.Core;

public interface IUsersRepository
{
    Task<User?> GetAsync(IUserInfo user);
    Task<IReadOnlyCollection<User>> GetAllAsync();
    Task UpdateLanguageSettingAsync(IUserInfo user, LanguageCode newLanguage);
}