using CheckMade.Common.LangExt.MonadicWrappers;
using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface IRequestProcessor
{
    public static readonly string WelcomeToBot = 
        UiSm("Willkommen zum {0}Bot! Klick auf den Menüknopf oder tippe '/' um verfügbare Befehle zu sehen.");

    public Task<Attempt<string>> SafelyEchoAsync(InputMessage inputMessage);
}