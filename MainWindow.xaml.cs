using RaySharp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace RaySharp
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<CommandResult> searchResults = new ObservableCollection<CommandResult>();
        private Storyboard? resultsAppearAnimation;
        private Storyboard? resultsDisappearAnimation;

        private const double CompactWindowHeight = 60;
        private const double ExpandedWindowHeight = 330;
        private bool isAnimatingSize = false;

        private ResourceDictionary? defaultTheme;
        private ResourceDictionary? darkTheme;
        private ResourceDictionary? lightTheme;

        public bool isVisible { get; private set; } = false;
        public bool alwaysOnTop = true;

        private static readonly Regex MathExpressionRegex = new Regex(@"^[0-9\.\+\-\*/\%\^\(\)\s]+$", RegexOptions.Compiled);

        private static readonly Dictionary<string, string> CommandIconCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["google"] = "🌐",
            ["g"] = "🌐",
            ["open notepad"] = "📝",
            ["calculator"] = "🧮",
            ["calc"] = "🧮",
            ["file"] = "📄",
            ["f"] = "📄",
            ["settings"] = "⚙️",
            ["help"] = "❓",
            ["exit"] = "🚪",
            ["quit"] = "🚪",
            ["weather"] = "🌤️",
            ["clock"] = "⏰",
            ["time"] = "⏰",
            ["music"] = "🎵",
            ["play"] = "🎵",
            ["video"] = "🎬",
            ["email"] = "📧",
            ["calendar"] = "📅",
            ["translate"] = "🌍",
            ["screenshot"] = "📸",
            ["timer"] = "⏲️",
            ["alarm"] = "⏰",
            ["map"] = "🗺️",
            ["maps"] = "🗺️"
        };

        private static readonly string[] CommonPaths = {
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
            Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
            Path.Combine("C:\\Users", Environment.UserName, "Downloads")
        };

        public MainWindow()
        {
            InitializeComponent();
            CommandRegistry.InitDefaults();

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            ResultsList.ItemsSource = searchResults;

            InitializeThemes();
            ApplyInitialTheme();

            CacheAnimations();
        }

        private void CacheAnimations()
        {
            resultsAppearAnimation = this.FindResource("ResultsAppearAnimation") as Storyboard;
            resultsDisappearAnimation = this.FindResource("ResultsDisappearAnimation") as Storyboard;

            if (resultsDisappearAnimation != null)
            {
                resultsDisappearAnimation.Completed += ResultsDisappearAnimation_Completed;
            }
        }

        private void ApplyInitialTheme()
        {
            try
            {
                var settings = SettingsManager.LoadSettings();
                ApplyTheme(settings.SearchTheme);
                alwaysOnTop = true;
            }
            catch
            {
                ApplyTheme("Default");
            }
        }

        private void InitializeThemes()
        {
            defaultTheme = new ResourceDictionary();

            ReadOnlySpan<object> resourceKeys = new object[] {
                "WindowBackground", "HeaderBarBackground", "SearchBoxTextColor", "SearchIconColor",
                "ResultsTitleColor", "ResultsDescriptionColor", "ResultsShortcutColor", "ResultsIconColor",
                "ResultsSelectionColor", "ResultsHoverColor", "ResultsSeparatorColor", "SearchBoxFocusColor"
            };

            foreach (var key in resourceKeys)
            {
                defaultTheme[key] = this.Resources[key];
            }

            darkTheme = CreateDarkTheme();
            lightTheme = CreateLightTheme();
        }

        private ResourceDictionary CreateDarkTheme()
        {
            var theme = new ResourceDictionary();

            Color purpleColor = Color.FromRgb(140, 60, 231);
            var purpleSelectionBrush = new SolidColorBrush(Color.FromArgb(61, purpleColor.R, purpleColor.G, purpleColor.B));
            var iconColor = new SolidColorBrush(Color.FromRgb(142, 142, 142));

            var windowBackground = new LinearGradientBrush
            {
                StartPoint = new Point(0.5, 0),
                EndPoint = new Point(0.5, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(20, 20, 22), 0),
                    new GradientStop(Color.FromRgb(10, 10, 12), 1)
                }
            };

            var headerBackground = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(purpleColor, 0),
                    new GradientStop(Color.FromRgb(107, 107, 255), 0.5),
                    new GradientStop(purpleColor, 1)
                }
            };

            theme["WindowBackground"] = windowBackground;
            theme["HeaderBarBackground"] = headerBackground;
            theme["SearchBoxTextColor"] = Brushes.White;
            theme["SearchIconColor"] = iconColor;
            theme["ResultsTitleColor"] = Brushes.White;
            theme["ResultsDescriptionColor"] = new SolidColorBrush(Color.FromRgb(170, 170, 170));
            theme["ResultsShortcutColor"] = iconColor;
            theme["ResultsIconColor"] = iconColor;
            theme["ResultsSelectionColor"] = purpleSelectionBrush;
            theme["ResultsHoverColor"] = new SolidColorBrush(Color.FromArgb(31, 255, 255, 255));
            theme["ResultsSeparatorColor"] = new SolidColorBrush(Color.FromArgb(38, 255, 255, 255));
            theme["SearchBoxFocusColor"] = purpleColor;

            return theme;
        }

        private ResourceDictionary CreateLightTheme()
        {
            var theme = new ResourceDictionary();

            Color blueColor = Color.FromRgb(52, 152, 219);
            var blueSelectionBrush = new SolidColorBrush(Color.FromArgb(61, blueColor.R, blueColor.G, blueColor.B));
            var iconColor = new SolidColorBrush(Color.FromRgb(100, 100, 100));

            var windowBackground = new LinearGradientBrush
            {
                StartPoint = new Point(0.5, 0),
                EndPoint = new Point(0.5, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(245, 245, 245), 0),
                    new GradientStop(Color.FromRgb(235, 235, 235), 1)
                }
            };

            var headerBackground = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(blueColor, 0),
                    new GradientStop(Color.FromRgb(41, 128, 185), 0.5),
                    new GradientStop(blueColor, 1)
                }
            };

            theme["WindowBackground"] = windowBackground;
            theme["HeaderBarBackground"] = headerBackground;
            theme["SearchBoxTextColor"] = new SolidColorBrush(Color.FromRgb(50, 50, 50));
            theme["SearchIconColor"] = iconColor;
            theme["ResultsTitleColor"] = new SolidColorBrush(Color.FromRgb(50, 50, 50));
            theme["ResultsDescriptionColor"] = new SolidColorBrush(Color.FromRgb(90, 90, 90));
            theme["ResultsShortcutColor"] = iconColor;
            theme["ResultsIconColor"] = iconColor;
            theme["ResultsSelectionColor"] = blueSelectionBrush;
            theme["ResultsHoverColor"] = new SolidColorBrush(Color.FromArgb(31, 0, 0, 0));
            theme["ResultsSeparatorColor"] = new SolidColorBrush(Color.FromArgb(38, 0, 0, 0));
            theme["SearchBoxFocusColor"] = blueColor;

            return theme;
        }

        public void ApplyTheme(string themeName)
        {
            if (defaultTheme == null)
                InitializeThemes();

            ResourceDictionary? themeToApply;

            if (themeName != null && themeName.Equals("dark", StringComparison.OrdinalIgnoreCase))
                themeToApply = darkTheme;
            else if (themeName != null && themeName.Equals("light", StringComparison.OrdinalIgnoreCase))
                themeToApply = lightTheme;
            else
                themeToApply = defaultTheme;

            if (themeToApply != null)
            {
                foreach (var key in themeToApply.Keys)
                {
                    this.Resources[key] = themeToApply[key];
                }
            }
        }

        public void Window_OnLoaded(object sender, RoutedEventArgs e)
        {
            Storyboard? windowAppearAnimation = this.FindResource("WindowAppearAnimation") as Storyboard;
            Storyboard? windowDisappearAnimation = this.FindResource("WindowDisappearAnimation") as Storyboard;

            windowAppearAnimation?.Begin(this);

            if (windowDisappearAnimation != null)
            {
                windowDisappearAnimation.Completed += WindowDisappearAnimation_Completed;
            }

            ResizeWindowToContent(false);
        }

        private void WindowDisappearAnimation_Completed(object? sender, EventArgs args)
        {
            this.Hide();
        }

        private void ResultsDisappearAnimation_Completed(object? sender, EventArgs args)
        {
            ResultsArea.Visibility = Visibility.Collapsed;
            ResizeWindowToContent(false);
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    ExecuteSelectedCommand();
                    break;
                case Key.Escape:
                    HideWithAnimation();
                    break;
                case Key.Down:
                    if (ResultsList.Items.Count > 0)
                    {
                        ResultsList.SelectedIndex = (ResultsList.SelectedIndex + 1) % ResultsList.Items.Count;
                        ResultsList.ScrollIntoView(ResultsList.SelectedItem);
                        e.Handled = true;
                    }
                    break;
                case Key.Up:
                    if (ResultsList.Items.Count > 0)
                    {
                        ResultsList.SelectedIndex = (ResultsList.SelectedIndex - 1 + ResultsList.Items.Count) % ResultsList.Items.Count;
                        ResultsList.ScrollIntoView(ResultsList.SelectedItem);
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSearchResults();
        }

        private void ResultsList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ExecuteSelectedCommand();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                this.HideWithAnimation();
                e.Handled = true;
            }
        }

        private void ResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void ExecuteSelectedCommand()
        {
            var query = SearchBox.Text.Trim();

            if (ResultsList.SelectedItem is CommandResult selectedResult)
            {
                if (IsMathResult(selectedResult))
                {
                    OpenMathExplanationSite(query);
                    HideWithAnimation();
                    return;
                }

                if (selectedResult.Icon == "📄" && !string.IsNullOrEmpty(selectedResult.Description))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = selectedResult.Description,
                            UseShellExecute = true
                        });
                        HideWithAnimation();
                        return;
                    }
                    catch (Exception) { }
                }

                if (selectedResult.Command != null)
                {
                    selectedResult.Command.Execute(query);
                    HideWithAnimation();
                }
            }
            else
            {
                var match = CommandRegistry.Search(query).FirstOrDefault();
                if (match != null)
                {
                    match.Execute(query);
                    HideWithAnimation();
                }
            }
        }

        private static bool IsMathResult(CommandResult result)
        {
            return result.Icon == "🔢" && result.Description != null && result.Description.StartsWith("Result of");
        }

        private static void OpenMathExplanationSite(string expression)
        {
            try
            {
                string url = $"https://www.google.com/search?q={Uri.EscapeDataString(expression)}+math+explanation";
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch
            {
                string bingUrl = $"https://www.bing.com/search?q={Uri.EscapeDataString(expression)}+math+explanation";
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = bingUrl,
                    UseShellExecute = true
                });
            }
        }

        private void UpdateSearchResults()
        {
            string query = SearchBox.Text.Trim();
            searchResults.Clear();

            if (string.IsNullOrWhiteSpace(query))
            {
                HideResultsArea();
                return;
            }

            if (TryEvaluateMathExpression(query, out string result))
            {
                searchResults.Add(new CommandResult
                {
                    Icon = "🔢",
                    Title = result,
                    Description = $"Result of {query}",
                    Shortcut = string.Empty,
                    Command = CommandRegistry.Search(query).FirstOrDefault()
                });
            }

            ReadOnlySpan<char> querySpan = query.AsSpan();
            if ((querySpan.Length > 2 && querySpan[0] == 'f' && querySpan[1] == ' ') ||
                (querySpan.Length > 5 && (querySpan[0] == 'f' || querySpan[0] == 'F') &&
                 (querySpan[1] == 'i' || querySpan[1] == 'I') && (querySpan[2] == 'l' || querySpan[2] == 'L') &&
                 (querySpan[3] == 'e' || querySpan[3] == 'E') && querySpan[4] == ' '))
            {
                ProcessFileSearch(query);
            }

            AddCommandResults(query);

            if (searchResults.Count > 0)
            {
                ResultsList.SelectedIndex = 0;
                ShowResultsArea();
            }
            else
            {
                HideResultsArea();
            }
        }

        private void ProcessFileSearch(string query)
        {
            ReadOnlySpan<char> querySpan = query.AsSpan();
            string searchTerm;

            if (querySpan[0] == 'f' || querySpan[0] == 'F')
            {
                if (querySpan.Length > 2 && querySpan[1] == ' ')
                    searchTerm = query.Substring(2).Trim();
                else if (querySpan.Length > 5)
                    searchTerm = query.Substring(5).Trim();
                else
                    return;
            }
            else
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                try
                {
                    var fileSearchResults = SearchFiles(searchTerm);
                    int count = Math.Min(fileSearchResults.Count, 10);

                    for (int i = 0; i < count; i++)
                    {
                        var fileResult = fileSearchResults[i];
                        searchResults.Add(new CommandResult
                        {
                            Icon = "📄",
                            Title = Path.GetFileName(fileResult),
                            Description = fileResult,
                            Shortcut = string.Empty,
                            Command = new FileSearchCommand()
                        });
                    }
                }
                catch (Exception ex)
                {
                    searchResults.Add(new CommandResult
                    {
                        Icon = "⚠️",
                        Title = "Error searching files",
                        Description = ex.Message,
                        Shortcut = string.Empty,
                        Command = null
                    });
                }
            }
        }

        private void AddCommandResults(string query)
        {
            var results = CommandRegistry.Search(query);

            var addedCommands = new HashSet<string>(capacity: 20, StringComparer.OrdinalIgnoreCase);

            foreach (var command in results)
            {
                if (addedCommands.Add(command.Name))
                {
                    searchResults.Add(new CommandResult
                    {
                        Icon = GetIconForCommand(command),
                        Title = command.Name,
                        Description = command.Description,
                        Shortcut = string.Empty,      
                        Command = command
                    });
                }
            }
        }

        private List<string> SearchFiles(string searchTerm)
        {
            List<string> results = new List<string>(15);
            searchTerm = searchTerm.ToLowerInvariant();

            List<string> searchPaths = new List<string>(CommonPaths.Length + 5);

            foreach (var path in CommonPaths)
            {
                if (Directory.Exists(path))
                    searchPaths.Add(path);
            }

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (!drive.IsReady || drive.DriveType != DriveType.Fixed)
                    continue;

                bool alreadyIncluded = false;
                string driveRoot = drive.RootDirectory.FullName;

                foreach (var path in CommonPaths)
                {
                    if (path.StartsWith(driveRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        alreadyIncluded = true;
                        break;
                    }
                }

                if (!alreadyIncluded)
                {
                    searchPaths.Add(driveRoot);
                }
            }

            foreach (var basePath in searchPaths)
            {
                try
                {
                    SearchDirectoryForFiles(basePath, searchTerm, results);
                    if (results.Count >= 15) break;

                    SearchForDirectories(basePath, searchTerm, results);
                    if (results.Count >= 15) break;

                    SearchSubdirectoriesForFiles(basePath, searchTerm, results);
                }
                catch { }      

                if (results.Count >= 15) break;
            }

            if (results.Count > 0)
            {
                HashSet<string> uniqueResults = new HashSet<string>(results, StringComparer.OrdinalIgnoreCase);
                if (uniqueResults.Count < results.Count)
                {
                    return new List<string>(uniqueResults);
                }
            }

            return results;
        }

        private static void SearchDirectoryForFiles(string basePath, string searchTerm, List<string> results)
        {
            try
            {
                string[] files = Directory.GetFiles(basePath, "*" + searchTerm + "*", SearchOption.TopDirectoryOnly);

                foreach (string file in files)
                {
                    if (Path.GetFileName(file).Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(file);
                        if (results.Count >= 5) break;
                    }
                }
            }
            catch { }      
        }

        private static void SearchForDirectories(string basePath, string searchTerm, List<string> results)
        {
            try
            {
                string[] directories = Directory.GetDirectories(basePath, "*" + searchTerm + "*", SearchOption.TopDirectoryOnly);

                foreach (string dir in directories)
                {
                    if (Path.GetFileName(dir).Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(dir);
                        if (results.Count >= 5) break;
                    }
                }
            }
            catch { }      
        }

        private static void SearchSubdirectoriesForFiles(string basePath, string searchTerm, List<string> results)
        {
            try
            {
                foreach (var dir in Directory.GetDirectories(basePath))
                {
                    try
                    {
                        string[] files = Directory.GetFiles(dir, "*" + searchTerm + "*", SearchOption.TopDirectoryOnly);

                        foreach (string file in files)
                        {
                            if (Path.GetFileName(file).Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            {
                                results.Add(file);
                                if (results.Count >= 3) return;
                            }
                        }
                    }
                    catch { }      
                }
            }
            catch { }      
        }

        private void ShowResultsArea()
        {
            if (ResultsArea.Visibility != Visibility.Visible)
            {
                ResultsArea.Visibility = Visibility.Visible;
                ResultsArea.Opacity = 0;
                ResizeWindowToContent(true);
            }

            if (resultsAppearAnimation != null)
            {
                resultsAppearAnimation.Begin();
            }
            else
            {
                ResultsArea.Opacity = 1;
            }
        }

        private void HideResultsArea()
        {
            if (ResultsArea.Visibility != Visibility.Visible)
                return;

            if (resultsDisappearAnimation != null)
            {
                resultsDisappearAnimation.Begin();
            }
            else
            {
                ResultsArea.Visibility = Visibility.Collapsed;
                ResizeWindowToContent(false);
            }
        }

        private void ResizeWindowToContent(bool showingResults)
        {
            if (isAnimatingSize)
                return;

            isAnimatingSize = true;

            double targetHeight = showingResults ? ExpandedWindowHeight : CompactWindowHeight;

            var animation = new DoubleAnimation
            {
                From = Height,
                To = targetHeight,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            animation.Completed += (s, e) => isAnimatingSize = false;
            BeginAnimation(HeightProperty, animation);
        }

        private static bool TryEvaluateMathExpression(string expression, out string result)
        {
            result = string.Empty;
            try
            {
                expression = expression.Replace("×", "*").Replace("÷", "/");

                if (MathExpressionRegex.IsMatch(expression))
                {
                    var dt = new DataTable();
                    var value = dt.Compute(expression, null);
                    result = value?.ToString() ?? string.Empty;
                    return !string.IsNullOrEmpty(result);
                }
            }
            catch { }     

            return false;
        }

        private string GetIconForCommand(ICommand command)
        {
            if (command is FileSearchCommand)
                return "📄";

            string commandTypeName = command.GetType().Name;
            if (commandTypeName == "FileSearchCommand")
                return "📄";

            if (CommandIconCache.TryGetValue(command.Name, out string icon))
            {
                return icon;
            }

            return "🔍";
        }

        private static string GetShortcutForCommand(ICommand command)
        {
            return string.Empty;
        }

        public void ShowLauncher()
        {
            Show();
            Activate();
            SearchBox.Clear();
            SearchBox.Focus();
            searchResults.Clear();
            HideResultsArea();
            ResizeWindowToContent(false);
            isVisible = true;

            Topmost = alwaysOnTop;

            Storyboard? windowAppearAnimation = this.FindResource("WindowAppearAnimation") as Storyboard;
            windowAppearAnimation?.Begin(this);
        }

        private void HideWithAnimation()
        {
            Storyboard? windowDisappearAnimation = this.FindResource("WindowDisappearAnimation") as Storyboard;
            if (windowDisappearAnimation != null)
            {
                windowDisappearAnimation.Begin(this);
                isVisible = false;
            }
            else
            {
                Hide();
                isVisible = false;
            }
        }
    }

    public class CommandResult
    {
        public string Icon { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Shortcut { get; set; } = string.Empty;
        public ICommand? Command { get; set; }
    }
}
