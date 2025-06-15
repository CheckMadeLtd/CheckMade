using CheckMade.Common.Domain.Data.Core.Actors;
using CheckMade.Common.Domain.Interfaces.Data.Core;
using CheckMade.Common.Utils.UiTranslation;

namespace CheckMade.Common.Domain.Interfaces.Persistence.Core;

public interface IUsersRepository
{
    Task<User?> GetAsync(IUserInfo user);
    Task<IReadOnlyCollection<User>> GetAllAsync();
    Task UpdateLanguageSettingAsync(IUserInfo user, LanguageCode newLanguage);
}