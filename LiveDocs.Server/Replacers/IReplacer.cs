using System.Threading.Tasks;

namespace LiveDocs.Server.Replacers
{
    public interface IReplacer
    {
        Task<string> Render(string dbAndStoredProcName);
    }
}