using System.Collections.Generic;

namespace Lollipop.Spider
{
    public interface IProduceSummoners
    {
        IEnumerable<long> Produce(int freshnessInMinutes, int limit);
    }
}
