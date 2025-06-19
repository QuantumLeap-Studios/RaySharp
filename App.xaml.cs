using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using NHotkey;
using NHotkey.Wpf;
using RaySharp.Services;
using System.Linq;
using System.IO;

namespace RaySharp
{
    public partial class App : Application
    {
        private MainWindow launcher;

        public static Mutex appMutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                SettingsManager.LoadSettings();

                launcher = new MainWindow();

                bool isNewInstance;
                appMutex = new Mutex(true, "QuantumLeap_RaySharp_Mutex", out isNewInstance);

                if (!isNewInstance)
                {
                    MessageBox.Show("Another instance of RaySharp is already running.",
                                    "Instance Detected", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Shutdown();
                    return;
                }

                RegisterHotkey();
            }
            catch (Exception ex)
            {
                try
                {
                    string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RaySharp");
                    Directory.CreateDirectory(appDataPath);

                    string logFile = Path.Combine(appDataPath, "crash.log");
                    File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}\n\n");

                    MessageBox.Show(
                        $"RaySharp encountered a startup issue.\n\n" +
                        $"Details have been logged to:\n{logFile}\n\n" +
                        $"Error: {ex.Message}",
                        "Unexpected Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
                catch
                {
                    MessageBox.Show(
                        "RaySharp failed to start due to an unexpected error, and logging could not be completed.",
                        "Critical Startup Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }

                Shutdown();
            }
        }

        private void RegisterHotkey()
        {
            try
            {
                var settings = SettingsManager.LoadSettings();

                ModifierKeys modifierKey = ModifierKeys.Control;  
                if (Enum.TryParse(settings.HotkeyModifier, true, out ModifierKeys parsedModifier))
                {
                    modifierKey = parsedModifier;
                }

                Key key = Key.Space;  
                if (Enum.TryParse(settings.HotkeyKey, true, out Key parsedKey))
                {
                    key = parsedKey;
                }

                HotkeyManager.Current.AddOrReplace("ShowLauncher", key, modifierKey, (s, args) =>
                {
                    if (!launcher.isVisible)
                    {
                        launcher.ShowLauncher();
                        args.Handled = true;
                    }
                });
            }
            catch (NHotkey.HotkeyAlreadyRegisteredException)
            {
                var currentProcess = Process.GetCurrentProcess();
                var others = Process.GetProcessesByName(currentProcess.ProcessName)
                                    .Where(p => p.Id != currentProcess.Id);

                foreach (var p in others)
                {
                    Debug.WriteLine($"Other instance: {p.Id} - {p.MainWindowTitle}");
                }

                try
                {
                    HotkeyManager.Current.AddOrReplace("ShowLauncher", Key.Space, ModifierKeys.Alt, (s, args) =>
                    {
                        if (!launcher.isVisible)
                        {
                            launcher.ShowLauncher();
                            args.Handled = true;
                        }
                    });
                    MessageBox.Show("The configured hotkey is already registered by another application. Using Alt+Space instead.", "Hotkey Changed", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (NHotkey.HotkeyAlreadyRegisteredException)
                {
                    MessageBox.Show("Both the configured hotkey and Alt+Space are already registered by other applications. Please change the hotkey in settings.", "Hotkey Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            HotkeyManager.Current.Remove("ShowLauncher");
            if (appMutex != null)
            {
                appMutex.ReleaseMutex();
                appMutex.Dispose();
            }
            base.OnExit(e);
        }
    }
}
