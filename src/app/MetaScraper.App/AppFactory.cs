using Domain.Abstraction;

namespace MetaScraper.App;

static class AppFactory
{
    public static BaseApp CreateApp()
    {
        return new App();
    }
    public static BaseApp CreateApp(string withName)
    {
        return new App(withName);
    }
}