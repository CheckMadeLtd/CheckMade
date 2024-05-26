namespace CheckMade.Tests.Startup.ConfigProviders;

internal class PrdDbConnStringProvider(string connString)
{
    public string Get => connString;
}
