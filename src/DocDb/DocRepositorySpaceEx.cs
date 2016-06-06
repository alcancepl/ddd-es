using System.Threading.Tasks;

namespace DocDb
{
    public static class DocRepositorySpaceEx
    {
        public static /*async*/ Task Recreate(this IDocRepositorySpace space)
        {
            return Task.CompletedTask;
        }
    }
}
