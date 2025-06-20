using CheckMade.Abstract.Domain.Model.Common.Actors;
using General.Utils.UiTranslation;

namespace CheckMade.Abstract.Domain.ServiceInterfaces.Persistence.Common;

public interface IUsersRepository
{
    Task<User?> GetAsync(IUserInfo user);
    Task<IReadOnlyCollection<User>> GetAllAsync();
    Task UpdateLanguageSettingAsync(IUserInfo user, LanguageCode newLanguage);
}