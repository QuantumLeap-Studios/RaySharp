using System;
using System.Windows;
using System.Windows.Input;
using NHotkey;
using NHotkey.Wpf;
using RaySharp.Services;

namespace RaySharp
{
    public partial class App : Application
    {
        private MainWindow launcher;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SettingsManager.LoadSettings();

            launcher = new MainWindow();
            HotkeyManager.Current.AddOrReplace("ShowLauncher", Key.Space, ModifierKeys.Control, (s, args) =>
            {
                launcher.ShowLauncher();
                args.Handled = true;
            });
        }
    }
}