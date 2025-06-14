using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Actors;
using CheckMade.Common.Model.Core.Actors.Concrete;

namespace CheckMade.Common.DomainModel.Interfaces.Persistence.Core;

public interface IUsersRepository
{
    Task<User?> GetAsync(IUserInfo user);
    Task<IReadOnlyCollection<User>> GetAllAsync();
    Task UpdateLanguageSettingAsync(IUserInfo user, LanguageCode newLanguage);
}