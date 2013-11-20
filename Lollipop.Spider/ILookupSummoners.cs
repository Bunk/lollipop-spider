using System.Threading.Tasks;
using Lollipop.Spider.Data.Domain;

namespace Lollipop.Spider
{
    public interface ILookupSummoners
    {
        Task<Summoner> Hydrate(long id);

        Task<Summoner> Lookup(long id);
    }
}