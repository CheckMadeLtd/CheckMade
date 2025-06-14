namespace CheckMade.Common.DomainModel.ChatBot.UserInteraction.BotCommands;

public static class TlgStart
{
    public const string Command = "/start"; // Independent of language, determined by Telegram, hence not Ui()
    public const int CommandCode = 1;
}