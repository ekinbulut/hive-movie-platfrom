namespace Infrastructure.Integration.Services.JellyFin;

public interface IJellyFinServiceConfiguration
{
    string BaseUrl { get; set; }
    string ApiKey { get; set; }
}

public class JellyFinServiceConfiguration : IJellyFinServiceConfiguration
{
    public string BaseUrl { get; set; }
    public string ApiKey { get; set; }
}