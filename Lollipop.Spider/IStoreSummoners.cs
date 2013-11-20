using Lollipop.Spider.Data.Domain;

namespace Lollipop.Spider
{
    public interface IStoreSummoners
    {
        void Store(Summoner summoner);

        void StoreWhenMissing(Summoner summoner);
    }
}