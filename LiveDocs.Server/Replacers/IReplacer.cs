namespace LiveDocs.Server.Replacers
{
    public interface IReplacer
    {
        string Render(string dbAndStoredProcName);
    }
}