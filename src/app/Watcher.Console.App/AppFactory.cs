using Domain.Abstraction;

namespace Watcher.Console.App;

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