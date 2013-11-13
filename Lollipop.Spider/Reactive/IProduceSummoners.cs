using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using com.riotgames.platform.statistics;
using log4net;
using Lollipop.Services;
using Lollipop.Spider.Data.Domain;
using LoveSeat;
using Strilanc.Value;

namespace Lollipop.Spider.Reactive
{
    public interface IProduceSummoners
    {
        IEnumerable<long> Produce(int freshnessInMinutes, int limit);
    }

    public interface ILookupSummoners
    {
        Task<Summoner> Lookup(long id);
    }

    public interface IStoreSummoners
    {
        void Store(Summoner summoner);
    }

    public interface IPollSummoners
    {
        int Freshness { get; set; }
        int MaxPollingFrequencyInSeconds { get; set; }
        int MaxResults { get; set; }
        IObservable<long> BeginPolling();
    }

    public class SummonerLookup : ILookupSummoners
    {
        private readonly ISummonerService _summonerService;

        public SummonerLookup(ISummonerService summonerService)
        {
            _summonerService = summonerService;
        }

        public async Task<Summoner> Lookup(long id)
        {
            // Since there's not already a summoner in the database, let's add it
            var name = await _summonerService.NamesById((int)id);
            var acct = await _summonerService.Get(name[0]);

            return new Summoner
            {
                Id = id,
                AccountId = acct.acctId,
                ProfileIconId = acct.profileIconId,
                Name = acct.name,
                InternalName = acct.internalName,
                RevisionDate = acct.revisionDate,
                Level = acct.summonerLevel
            };
        }
    }

    public class SummonerProducer : IProduceSummoners, IStoreSummoners
    {
        public static readonly ILog Log = LogManager.GetLogger(typeof (SummonerProducer));

        public static string DatabaseName = "summoners";
        public static string IndexName = "indexes";

        private readonly CouchDatabase _summoners;

        public SummonerProducer(CouchClient db)
        {
            _summoners = db.GetDatabase(DatabaseName);
            _summoners.SetDefaultDesignDoc(IndexName);
        }

        public IEnumerable<long> Produce(int freshnessInMinutes, int limit)
        {
            try
            {
                var options = new ViewOptions { Limit = limit };
                options.EndKey.Add(freshnessInMinutes);

                return GetSummonerIds(options);
            }
            catch (Exception ex)
            {
                Log.Error("Error reading from the CouchDB index.", ex);
                throw;
            }
        }

        public Task<IEnumerable<long>> ProduceAsync(int freshnessInMinutes, int limit)
        {
            var source = new TaskCompletionSource<IEnumerable<long>>();

            try
            {
                var options = new ViewOptions { Limit = limit };
                options.EndKey.Add(freshnessInMinutes);

                source.TrySetResult(GetSummonerIds(options));
            }
            catch (Exception ex)
            {
                Log.Error("Error reading from the CouchDB index.", ex);
                source.TrySetException(ex);
            }

            return source.Task;
        }

        private IEnumerable<long> GetSummonerIds(ViewOptions options)
        {
            return _summoners.View<long[]>("awaiting_refresh", options)
                .Items.Select(el => el[1]);
        }

        public void Store(Summoner summoner)
        {
            _summoners.SaveDocument(new Document<Summoner>(summoner)
            {
                Id = summoner.Id.ToString(CultureInfo.InvariantCulture)
            });
        }
    }

    public class SummonerPollingAgent : IPollSummoners
    {
        public int Freshness { get; set; }

        public int MaxPollingFrequencyInSeconds { get; set; }

        public int MaxResults { get; set; }

        private readonly IProduceSummoners _summonerProducer;

        public SummonerPollingAgent(IProduceSummoners summonerProducer)
        {
            Freshness = (int) TimeSpan.FromDays(1).TotalMinutes;
            MaxPollingFrequencyInSeconds = 30;
            MaxResults = 30;

            _summonerProducer = summonerProducer;
        } 

        public IObservable<long> BeginPolling()
        {
            var frequency = TimeSpan.FromSeconds(MaxPollingFrequencyInSeconds);

            return Observable
                .Create<long>(observer =>
                              NewThreadScheduler
                                  .Default
                                  .SchedulePeriodic(frequency, PullData(observer, Freshness, MaxResults)));
        }

        private Action PullData(IObserver<long> observer, int freshnessFilter, int maxResults)
        {
            return () =>
            {
                var found = _summonerProducer.Produce(freshnessFilter, maxResults);
                foreach (var item in found)
                    observer.OnNext(item);
            };
        }
    }

    public class SummonerCrawler
    {
        private readonly ISummonerService _summonerService;
        private readonly IStatsService _statsService;
        private readonly IStoreSummoners _summoners;

        public SummonerCrawler(IStoreSummoners store, ISummonerService summonerService, IStatsService statsService)
        {
            _summonerService = summonerService;
            _statsService = statsService;

            _summoners = store;
        }

        public async Task<IObservable<long>> Crawl(long id)
        {
            await Hydrate(id);
            return await CrawlGames(id);
        }

        private async Task<long> Hydrate(long id)
        {
            var summoner = await _summonerService.PublicData((int)id);
            var named = await _summonerService.Get(summoner.summoner.name);
            //var kudos = await _summonerService.Kudos((int) id);
            //var stats = await _statsService.GetLifetimeStats((int) id);
            //var stats2 = await _statsService.GetAggregatedStats((int) id, GameMode.CLASSIC);
            //var champs = await _statsService.GetMostPlayedChamps((int) id, GameMode.CLASSIC);

            _summoners.Store(new Summoner
            {
                Id = summoner.summoner.sumId,
                AccountId = summoner.summoner.acctId,
                ProfileIconId = summoner.summoner.profileIconId,
                Name = summoner.summoner.name,
                InternalName = summoner.summoner.internalName,
                Level = summoner.summonerLevel.summonerLevel,
                RevisionDate = named.revisionDate,
                LastCrawledDate = DateTime.UtcNow
            });

            return id;
        }

        private async Task<IObservable<long>> CrawlGames(long id)
        {
            var recent = await _statsService.GetRecentGames((int)id);

            // todo: Aggregate into a single document for storage
            // todo: Store the aggregated game

            var set = new HashSet<long>();
            foreach (var game in recent.gameStatistics)
            {
                var data = new Game {Id = game.id.ToString()};
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

                    set.Add(participant.summonerId);
                }
                // todo: Save the game data
            }
            return set.ToObservable();
        }
    }
}
