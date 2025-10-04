using System;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;
using fast_cli_tool.Services;

namespace fast_cli_tool;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private readonly LogService _logService;

    public App()
    {
        _logService = new LogService();

        // 捕捉所有未處理的例外
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        _logService.LogError("Unhandled UI exception", e.Exception);

        System.Windows.MessageBox.Show(
            $"An unexpected error occurred:\n{e.Exception.Message}\n\nLog file: {_logService.GetLogFilePath()}",
            "Unexpected Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error
        );

        e.Handled = true; // 防止應用程式崩潰
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            _logService.LogError("Unhandled application exception", ex);

            System.Windows.MessageBox.Show(
                $"A critical error occurred:\n{ex.Message}\n\nLog file: {_logService.GetLogFilePath()}",
                "Critical Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }
}

