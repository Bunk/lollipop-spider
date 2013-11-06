using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Transactions;
using Lollipop.Services;

namespace Lollipop.Spider.Workers
{
    public class SummonerIdPool
    {
        
    }

    public class SummonerProducer
    {
        
    }

    public class SummonerConsumer
    {
        private readonly Func<ChampionCrawler> _factory;

        public SummonerConsumer(Func<ChampionCrawler> factory)
        {
            _factory = factory;
        }

        public void Execute(CancellationToken token)
        {
            var consumers = new ActionBlock<long>(id => Process(id), new ExecutionDataflowBlockOptions
            {
                CancellationToken = token,
                MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded
            });

            // Retrieve the top page of summoner ids to process
            var ids = new List<long>();
            foreach (var id in ids)
            {
                consumers.Post(id);
            }
        }

        private async Task Process(long id)
        {
            await _factory().Crawl(id);
        }
    }
}
