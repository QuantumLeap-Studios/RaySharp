using Microsoft.Win32;
using RaySharp.Models;
using RaySharp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RaySharp
{
    public partial class SettingsWindow : Window
    {
        private Settings _settings;
        private ObservableCollection<PluginViewModel> _plugins = new ObservableCollection<PluginViewModel>();

        public SettingsWindow()
        {
            InitializeComponent();

            _settings = SettingsManager.LoadSettings();

            ShowAnimationCheckBox.IsChecked = _settings.ShowAnimation;
            AlwaysOnTopCheckBox.IsChecked = _settings.AlwaysOnTop;
            MaxResultsComboBox.SelectedIndex = GetMaxResultsIndex(_settings.MaxSearchResults);
            ThemeComboBox.SelectedItem = ThemeComboBox.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == _settings.SearchTheme);

            HotkeyModifierComboBox.SelectedItem = HotkeyModifierComboBox.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == _settings.HotkeyModifier);
            HotkeyKeyComboBox.SelectedItem = HotkeyKeyComboBox.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == _settings.HotkeyKey);

            LoadPlugins();

            PluginListView.ItemsSource = _plugins;
        }

        private void LoadPlugins()
        {
            _plugins.Clear();

            foreach (string pluginPath in _settings.PluginPaths)
            {
                try
                {
                    var fileName = Path.GetFileName(pluginPath);
                    var isEnabled = _settings.EnabledPlugins.Contains(pluginPath);

                    _plugins.Add(new PluginViewModel
                    {
                        Path = pluginPath,
                        Name = Path.GetFileNameWithoutExtension(fileName),
                        Description = $"Plugin from {pluginPath}",
                        IsEnabled = isEnabled
                    });
                }
                catch
                {
                }
            }
        }

        private int GetMaxResultsIndex(int maxResults)
        {
            switch (maxResults)
            {
                case 5: return 0;
                case 10: return 1;
                case 20: return 3;
                case 30: return 4;
                default: return 2;
            }
        }

        private int GetMaxResultsValue(int index)
        {
            switch (index)
            {
                case 0: return 5;
                case 1: return 10;
                case 3: return 20;
                case 4: return 30;
                default: return 15;
            }
        }

        private void ImportPluginButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Plugin Files (*.dll)|*.dll",
                Title = "Select a Plugin"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string pluginPath = openFileDialog.FileName;

                if (_settings.PluginPaths.Contains(pluginPath))
                {
                    MessageBox.Show("This plugin is already imported.", "Import Plugin",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                try
                {
                    string pluginDir = SettingsManager.GetPluginDirectory();
                    string destinationPath = Path.Combine(pluginDir, Path.GetFileName(pluginPath));

                    File.Copy(pluginPath, destinationPath, true);

                    _settings.PluginPaths.Add(destinationPath);
                    _settings.EnabledPlugins.Add(destinationPath);

                    _plugins.Add(new PluginViewModel
                    {
                        Path = destinationPath,
                        Name = Path.GetFileNameWithoutExtension(destinationPath),
                        Description = $"Plugin from {destinationPath}",
                        IsEnabled = true
                    });

                    MessageBox.Show("Plugin imported successfully.", "Import Plugin",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing plugin: {ex.Message}", "Import Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RemovePluginButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedPlugin = PluginListView.SelectedItem as PluginViewModel;
            if (selectedPlugin == null)
            {
                MessageBox.Show("Please select a plugin to remove.", "Remove Plugin",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to remove the plugin '{selectedPlugin.Name}'?",
                "Remove Plugin", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _settings.PluginPaths.Remove(selectedPlugin.Path);
                _settings.EnabledPlugins.Remove(selectedPlugin.Path);
                _plugins.Remove(selectedPlugin);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _settings.ShowAnimation = ShowAnimationCheckBox.IsChecked ?? true;
            _settings.AlwaysOnTop = AlwaysOnTopCheckBox.IsChecked ?? false;
            _settings.MaxSearchResults = GetMaxResultsValue(MaxResultsComboBox.SelectedIndex);
            _settings.SearchTheme = ((ComboBoxItem)ThemeComboBox.SelectedItem).Content.ToString();

            if (HotkeyModifierComboBox.SelectedItem != null && HotkeyKeyComboBox.SelectedItem != null)
            {
                string oldModifier = _settings.HotkeyModifier;
                string oldKey = _settings.HotkeyKey;

                _settings.HotkeyModifier = ((ComboBoxItem)HotkeyModifierComboBox.SelectedItem).Content.ToString();
                _settings.HotkeyKey = ((ComboBoxItem)HotkeyKeyComboBox.SelectedItem).Content.ToString();

                if (oldModifier != _settings.HotkeyModifier || oldKey != _settings.HotkeyKey)
                {
                    MessageBox.Show($"Hotkey changed to {_settings.HotkeyModifier}+{_settings.HotkeyKey}. " +
                                    "Please restart the application for the new hotkey to take effect.",
                                    "Hotkey Changed", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            _settings.EnabledPlugins.Clear();
            foreach (var plugin in _plugins.Where(p => p.IsEnabled))
            {
                _settings.EnabledPlugins.Add(plugin.Path);
            }

            SettingsManager.SaveSettings(_settings);

            ReloadPlugins();

            DialogResult = true;
            Close();
        }

        private void ReloadPlugins()
        {
            var enabledPluginPaths = _settings.PluginPaths.Where(p => _settings.EnabledPlugins.Contains(p)).ToList();
            var pluginCommands = PluginLoader.LoadPlugins(enabledPluginPaths);

            foreach (var command in pluginCommands)
            {
                CommandRegistry.Register(command);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class PluginViewModel
    {
        public required string Path { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public bool IsEnabled { get; set; }
    }
}
