using Microsoft.UI.Xaml;
using Microsoft.Windows.Storage;
using System;
using System.IO;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using YTM_Presence.Handler;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace YTM_Presence
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static MainWindow? MainWindow { get; private set; }
        public static TaskerManager? TaskerManager { get; private set; }
        //private static TrayIcon? TrayIconInstance;
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            TaskerManager = new TaskerManager();

            //TrayIconInstance = new TrayIcon();

            MainWindow = new MainWindow(TaskerManager);

            var activation = AppInstance.GetActivatedEventArgs();

            if (activation.Kind == ActivationKind.StartupTask)
            {
                return;
            }

            MainWindow.Activate();
        }
        public static void Cleanup()
        {
            TaskerManager?.Dispose();

            //TrayIconInstance?.Dispose();
        }
    }
}
