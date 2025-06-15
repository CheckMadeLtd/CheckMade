namespace CheckMade.Common.Domain.Data.ChatBot.UserInteraction.BotCommands;

public static class Start
{
    public const string Command = "/start"; // Independent of language, determined by Telegram, hence not Ui()
    public const int CommandCode = 1;
}