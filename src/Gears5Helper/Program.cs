namespace Gears5Helper;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Locale.Load();
        Application.Run(new MainForm());
    }
}
