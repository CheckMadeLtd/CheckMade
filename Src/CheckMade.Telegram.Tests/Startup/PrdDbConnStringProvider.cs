namespace CheckMade.Telegram.Tests.Startup;

internal class PrdDbConnStringProvider(string connString)
{
    public string Get => connString;
}
