using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lollipop.Services;

namespace Lollipop.Spider.Workers
{
    public class SummonerGame
    {
        public long Id { get; set; }

        public long GameId { get; set; }

        public DateTime GameDate { get; set; }
    }

    /// <summary>
    /// Given a list of summoner ids, this class is responsible for following the trail
    /// and returning a new list of related summoners.
    /// </summary>
    public class ActiveChampionCrawler
    {
        private readonly IStatsService _statsService;
        private Queue<long> _summonerIds;

        public ActiveChampionCrawler(IStatsService statsService)
        {
            if (statsService == null) throw new ArgumentNullException("statsService");

            _statsService = statsService;
        }

        public async Task<List<SummonerGame>> Crawl(List<long> summonerIds)
        {
            var crawled = new List<SummonerGame>();

            _summonerIds = new Queue<long>();

            while (_summonerIds.Count > 0)
            {
                var current = _summonerIds.Dequeue();

                var recentGames = await _statsService.GetRecentGames((int) current);

                crawled.AddRange(from game in recentGames.gameStatistics
                                 from player in game.fellowPlayers
                                 select new SummonerGame
                                 {
                                     Id = player.summonerId,
                                     GameId = game.gameId,
                                     GameDate = game.createDate
                                 });
            }

            return crawled;
        }
    }
}
