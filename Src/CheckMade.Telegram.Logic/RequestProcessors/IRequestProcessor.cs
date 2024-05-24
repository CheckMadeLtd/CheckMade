using CheckMade.Common.LangExt;
using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface IRequestProcessor
{
    public static readonly UiString WelcomeToBotMenuInstruction = 
        Ui("Klick auf den Menüknopf oder tippe '/' um verfügbare Befehle zu sehen.");

    public Task<Attempt<UiString>> SafelyEchoAsync(InputMessage inputMessage);
}