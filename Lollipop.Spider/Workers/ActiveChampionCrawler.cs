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

    public class ChampionCrawler
    {
        private readonly IStatsService _service;

        public ChampionCrawler(IStatsService service)
        {
            if (service == null) throw new ArgumentNullException("service");

            _service = service;
        }

        public async Task<List<SummonerGame>> Crawl(long id)
        {
            // todo: Update summoner data

            var recentGames = await _service.GetRecentGames((int) id);
            
            // todo: Update Games tables

            return (from game in recentGames.gameStatistics
                    from player in game.fellowPlayers
                    select new SummonerGame
                    {
                        Id = player.summonerId,
                        GameId = game.gameId,
                        GameDate = game.createDate
                    }).ToList();
        }
    }
}
