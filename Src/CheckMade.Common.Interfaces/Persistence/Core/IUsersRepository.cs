using CheckMade.Common.Model.Core;

namespace CheckMade.Common.Interfaces.Persistence.Core;

public interface IUsersRepository
{
    Task UpdateLanguageSettingAsync(User user, LanguageCode newLanguage);
}