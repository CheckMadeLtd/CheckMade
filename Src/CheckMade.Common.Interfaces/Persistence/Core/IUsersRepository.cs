using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Interfaces;

namespace CheckMade.Common.Interfaces.Persistence.Core;

public interface IUsersRepository
{
    Task<IReadOnlyCollection<User>> GetAllAsync();
    Task UpdateLanguageSettingAsync(IUserInfo user, LanguageCode newLanguage);
}