using System;
using System.Windows;
using System.Windows.Threading;
using ChatApp.Client.Views;

namespace ChatApp.Client
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Set up global exception handling
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            // Initialize application resources
            InitializeResources();
        }

        private void InitializeResources()
        {
            // Add any application-wide resources here
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/Resources/Styles.xaml")
            });
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            HandleException(e.Exception);
            e.Handled = true;
        }

        private void HandleException(Exception ex)
        {
            if (ex != null)
            {
                MessageBox.Show(
                    $"An error occurred: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Create and show the main window
            var loginWindow = new LoginWindow();
            loginWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Clean up any resources
            base.OnExit(e);
        }
    }
} 