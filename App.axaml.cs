using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EasySave.Services;
using EasySave.ViewModels;
using EasySave.Views;
using System;

namespace EasySave;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            try
            {
                var driveService = GoogleAuthService.AuthenticateAsync().GetAwaiter().GetResult();
                var syncService = new DriveSyncService(driveService);
                var saveSyncManager = new SaveSyncManager(syncService);

                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainViewModel(saveSyncManager, new FilePickerService())
                };
            }
            catch (Exception ex)
            {
                desktop.MainWindow = new Window
                {
                    Width = 400,
                    Height = 150,
                    Content = new TextBlock
                    {
                        Text = $"Startup failed:\n{ex.Message}\n\nCheck credentials.json and internet connection.",
                        Margin = new Avalonia.Thickness(20),
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    }
                };
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}