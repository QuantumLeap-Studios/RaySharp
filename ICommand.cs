public interface ICommand
{
    string Name { get; }
    string Description { get; }
    bool Matches(string query);
    void Execute(string query);
}
