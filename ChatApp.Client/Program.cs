using ChatApp.Client.Views;
using System.Windows;
namespace ChatApp.Client;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        var app = new System.Windows.Application();
        app.Run(new SignIn());
    }    
}