using CheckMade.Abstract.Domain.Data.Core.Actors;
using CheckMade.Abstract.Domain.Interfaces.Data.Core;
using CheckMade.Common.Utils.UiTranslation;

namespace CheckMade.Abstract.Domain.Interfaces.Persistence.Core;

public interface IUsersRepository
{
    Task<User?> GetAsync(IUserInfo user);
    Task<IReadOnlyCollection<User>> GetAllAsync();
    Task UpdateLanguageSettingAsync(IUserInfo user, LanguageCode newLanguage);
}