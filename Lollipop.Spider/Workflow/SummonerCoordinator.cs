using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Lollipop.Services;
using Lollipop.Spider.Data.Domain;
using LoveSeat;

namespace Lollipop.Spider.Workflow
{
    public class SummonerProducer
    {
        private readonly IStatsService _statsService;
        private readonly ISummonerService _summonerService;
        private readonly CouchDatabase _summoners;
        private readonly CouchDatabase _games;

        public SummonerProducer(CouchClient db,
                                IStatsService statsService,
                                ISummonerService summonerService)
        {
            _statsService = statsService;
            _summonerService = summonerService;

            _summoners = db.GetDatabase("summoners");
            _summoners.SetDefaultDesignDoc("indexes");

            _games = db.GetDatabase("games");
            _games.SetDefaultDesignDoc("indexes");
        }

        public Task Produce(CancellationToken token)
        {
            //var buffer = new BufferBlock<long>(new DataflowBlockOptions {CancellationToken = token});

            var store = new TransformBlock<long, long>(id => Task.Run(() => StoreSummoner(id), token),
                                                       new ExecutionDataflowBlockOptions {CancellationToken = token});
            var crawl = new TransformManyBlock<long, long>(id => Crawl(id),
                                                           new ExecutionDataflowBlockOptions {CancellationToken = token});
            var updateBatches = new ActionBlock<long>(id => AddSummoner(id),
                                                      new ExecutionDataflowBlockOptions {CancellationToken = token});

            //buffer.LinkTo(store, new DataflowLinkOptions {PropagateCompletion = true});
            store.LinkTo(crawl, new DataflowLinkOptions {PropagateCompletion = true});
            crawl.LinkTo(updateBatches, new DataflowLinkOptions {PropagateCompletion = true});

            return Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    var batch = await GetNextBatch();
                    foreach (var id in batch)
                    {
                        store.Post(id);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(15), token);
                }

                // Finalize and wait until all steps are complete
                crawl.Complete();
                await updateBatches.Completion;
            }, token);
        }

        private async Task<IEnumerable<long>> GetNextBatch()
        {
            var options = new ViewOptions {Limit = 30};
            options.EndKey.Add(TimeSpan.FromDays(1).TotalMinutes);

            try
            {
                var summoners = _summoners.View<long>("awaiting_refresh", options);
                return summoners.Items;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<long> StoreSummoner(long id)
        {
            var summoner = await _summonerService.PublicData((int) id);
            var named = await _summonerService.Get(summoner.summoner.name);
            //var kudos = await _summonerService.Kudos((int) id);
            //var stats = await _statsService.GetLifetimeStats((int) id);
            //var stats2 = await _statsService.GetAggregatedStats((int) id, GameMode.CLASSIC);
            //var champs = await _statsService.GetMostPlayedChamps((int) id, GameMode.CLASSIC);

            // todo: Map more aggregate data in here
            var data = new Summoner
            {
                Id = summoner.summoner.sumId,
                AccountId = summoner.summoner.acctId,
                ProfileIconId = summoner.summoner.profileIconId,
                Name = summoner.summoner.name,
                InternalName = summoner.summoner.internalName,
                Level = summoner.summonerLevel.summonerLevel,
                RevisionDate = named.revisionDate,
                LastCrawledDate = DateTime.UtcNow
            };

            _summoners.SaveDocument(new Document<Summoner>(data)
            {
                Id = summoner.summoner.sumId.ToString(CultureInfo.InvariantCulture)
            });

            return id;
        }

        private async Task<IEnumerable<long>> Crawl(long id)
        {
            var recent = await _statsService.GetRecentGames((int) id);

            // todo: Aggregate into a single document for storage
            // todo: Store the aggregated game

            var ids = new ConcurrentDictionary<long, object>();

            Parallel.ForEach(recent.gameStatistics, game =>
            {
                // todo: Aggregate more data in here
                var data = new Game
                {
                    Id = game.id.ToString()
                };

                foreach (var participant in game.fellowPlayers)
                {
                    var participantData = new Participant
                    {
                        SummonerId = participant.summonerId,
                        ChampionId = participant.championId
                    };

                    if (participant.teamId == 100)
                        data.TeamOne.Add(participantData);
                    else
                        data.TeamTwo.Add(participantData);

                    // Using a concurrent dictionary since there's no concurrent hash set
                    // implementation.  We don't want duplicates, and we don't care about
                    // the value--just the keys
                    ids.AddOrUpdate(participant.summonerId, l => null, (l, o) => null);
                }

                _games.SaveDocument(new Document<Game>(data)
                {
                    Id = game.id.ToString()
                });
            });

            return ids.Keys;
        }

        private async Task AddSummoner(long id)
        {
            var strId = id.ToString(CultureInfo.InvariantCulture);
            var existing = _summoners.GetDocument<Summoner>(strId);
            if (existing != null)
                return;

            // Since there's not already a summoner in the database, let's add it
            var name = await _summonerService.NamesById((int) id);
            var acct = await _summonerService.Get(name[0]);

            var data = new Summoner
            {
                Id = id,
                AccountId = acct.acctId,
                ProfileIconId = acct.profileIconId,
                Name = acct.name,
                InternalName = acct.internalName,
                RevisionDate = acct.revisionDate,
                Level = acct.summonerLevel
            };

            _summoners.SaveDocument(new Document<Summoner>(data)
            {
                Id = id.ToString(strId)
            });
        }
    }
}