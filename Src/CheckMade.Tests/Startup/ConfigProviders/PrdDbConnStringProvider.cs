namespace CheckMade.Tests.Startup.ConfigProviders;

internal sealed class PrdDbConnStringProvider(string connString)
{
    public string Get => connString;
}
