using System.Diagnostics;
using System.Web;

public class GoogleSearchCommand : ICommand
{
    public string Name => "Google";
    public string Description => "Search Google";

    public bool Matches(string query) =>
        query.StartsWith("g ", StringComparison.OrdinalIgnoreCase);

    public void Execute(string query)
    {
        var q = HttpUtility.UrlEncode(query.Substring(2));
        Process.Start(new ProcessStartInfo($"https://www.google.com/search?q={q}") { UseShellExecute = true });
    }
    public void Awake()
    {
    }
}
