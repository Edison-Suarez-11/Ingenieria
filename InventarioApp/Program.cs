namespace InventarioApp;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Data.Database.InitializeDatabase();
        Application.Run(new Forms.MainForm());
    }    
}