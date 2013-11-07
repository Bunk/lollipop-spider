using System.Threading.Tasks;

namespace Lollipop.Spider.Data
{
    public interface IRepository<T>
    {
        Task<T> Get(object id);

        Task Store(object id, T obj);
    }
}
