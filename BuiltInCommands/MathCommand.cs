using System;
using System.Data;
using System.Text.RegularExpressions;

public class MathCommand : ICommand
{
    private readonly Regex mathRegex = new Regex(@"^[\d\s\+\-\*\/\(\)\.\%]+$", RegexOptions.Compiled);

    public string Name => "Math";
    public string Description => "Calculate mathematical expressions";

    public bool Matches(string query)
    {
        return !string.IsNullOrEmpty(query) && mathRegex.IsMatch(query);
    }

    public void Execute(string query)
    {
        try
        {
            var dt = new DataTable();
            var result = dt.Compute(query, "");

        }
        catch (Exception)
        {
        }
    }
}
