using System.Configuration;
using System.Data;
using System.Windows;
using TrackerLibrary;

namespace TrackerUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Set up high DPI mode and enable visual styles (if necessary)
            // Note: WPF does not have direct equivalents for SetHighDpiMode and EnableVisualStyles methods.
            //       If you need these features, you might need to implement them manually or find alternatives.
            // Application.SetHighDpiMode(HighDpiMode.SystemAware);
            // Application.EnableVisualStyles();

            // Set compatible text rendering default
            // Application.SetCompatibleTextRenderingDefault(false);

            // Initialize the database connections
            GlobalConfig.InitializeConnections(DatabaseType.TextFile);

            // Create and show the main window
            TournamentDashboardForm mainWindow = new TournamentDashboardForm(); // Assuming MainWindow is the main window of your WPF application
            mainWindow.Show();
        }
    }

}
