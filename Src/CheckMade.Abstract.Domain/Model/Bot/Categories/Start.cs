namespace CheckMade.Abstract.Domain.Model.Bot.Categories;

public static class Start
{
    public const string Command = "/start"; // Independent of language, determined by Telegram, hence not Ui()
    public const int CommandCode = 1;
}