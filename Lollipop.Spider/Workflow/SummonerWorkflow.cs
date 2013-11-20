using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using com.riotgames.platform.statistics;
using Lollipop.Spider.Data.Domain;

namespace Lollipop.Spider.Workflow
{
    public class SummonerWorkflow : IDisposable
    {
        private readonly ICrawlSummoners _crawler;
        private readonly ILookupSummoners _lookup;
        private readonly IProduceSummoners _producer;
        private readonly IStoreSummoners _storage;
        private BufferBlock<long> _buffer;
        private bool _disposed;

        public SummonerWorkflow(IProduceSummoners producer, ILookupSummoners lookup, ICrawlSummoners crawler,
            IStoreSummoners storage)
        {
            _producer = producer;
            _lookup = lookup;
            _crawler = crawler;
            _storage = storage;
        }

        public Task Execute(CancellationToken token)
        {
            var options = new DataflowBlockOptions {CancellationToken = token};
            _buffer = new BufferBlock<long>(options);

            var hydrate = new TransformBlock<long, Summoner>(id =>
            {
                var summoner = _lookup.Hydrate(id);
                return summoner;
            }, new ExecutionDataflowBlockOptions { CancellationToken = token, MaxDegreeOfParallelism = 2 });

            var store = new TransformBlock<Summoner, Summoner>(summoner =>
            {
                if (summoner != null)
                    _storage.Store(summoner);

                return summoner;
            }, new ExecutionDataflowBlockOptions {CancellationToken = token, MaxDegreeOfParallelism = 2});

            var crawl = new TransformManyBlock<Summoner, FellowPlayerInfo>(async summoner =>
            {
                var summoners = new List<FellowPlayerInfo>();
                var games = new List<PlayerGameStats>();
                if (summoner != null)
                {
                    await _crawler.Crawl(summoner, summoners.Add, games.Add);
                }
                return summoners;
            }, new ExecutionDataflowBlockOptions {CancellationToken = token, MaxDegreeOfParallelism = 2});

            var storeNextBatch = new ActionBlock<FellowPlayerInfo>(async info =>
            {
                if (info != null)
                {
                    var data = await _lookup.Lookup(info.summonerId);
                    _storage.StoreWhenMissing(data);
                }
            }, new ExecutionDataflowBlockOptions {CancellationToken = token, MaxDegreeOfParallelism = 2});

            _buffer.LinkTo(hydrate, new DataflowLinkOptions {PropagateCompletion = true});
            hydrate.LinkTo(store, new DataflowLinkOptions {PropagateCompletion = true});
            store.LinkTo(crawl, new DataflowLinkOptions {PropagateCompletion = true});
            crawl.LinkTo(storeNextBatch, new DataflowLinkOptions {PropagateCompletion = true});

            return Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var batch = _producer.Produce((int) TimeSpan.FromDays(1).TotalMinutes, 30);
                        foreach (var id in batch)
                            await _buffer.SendAsync(id, token);

                        // Start the chain
                        _buffer.Complete();

                        // Wait until the chain is complete before iterating again
                        await storeNextBatch.Completion;
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }, token);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (!disposing) return;

            _disposed = true;
        }
    }
}