using System;
using System.Diagnostics;

public class PlaySongCommand : ICommand
{
    public string Name => "Music";
    public string Description => "Play Music";

    public bool Matches(string query) =>
        query.StartsWith("music ", StringComparison.OrdinalIgnoreCase) ||
        query.StartsWith("play ", StringComparison.OrdinalIgnoreCase) ||
        query.Equals("music", StringComparison.OrdinalIgnoreCase);

    public void Execute(string query)
    {
        string searchTerm;

        if (query.StartsWith("music ", StringComparison.OrdinalIgnoreCase))
            searchTerm = query.Substring(6);
        else if (query.StartsWith("play ", StringComparison.OrdinalIgnoreCase))
            searchTerm = query.Substring(5);
        else
            searchTerm = string.Empty;

        string url = string.IsNullOrWhiteSpace(searchTerm)
            ? "https://open.spotify.com"
            : $"https://open.spotify.com/search/{Uri.EscapeDataString(searchTerm)}";

        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
}
