using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.riotgames.platform.statistics;
using log4net;
using Lollipop.Services;
using Lollipop.Spider.Data.Domain;

namespace Lollipop.Spider
{
    public class SummonerCrawler : ICrawlSummoners
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (SummonerCrawler));

        private readonly ISummonerService _summonerService;
        private readonly IStatsService _statsService;

        public Action<FellowPlayerInfo> OnSummonerCrawled { get; set; }

        public Action<PlayerGameStats> OnGameCrawled { get; set; }

        public SummonerCrawler(ISummonerService summonerService, IStatsService statsService)
        {
            _summonerService = summonerService;
            _statsService = statsService;
        }

        public async Task Crawl(Summoner summoner, Action<FellowPlayerInfo> foundSummoner, Action<PlayerGameStats> foundGame)
        {
            if (summoner == null)
                return;

            var recent = await _statsService.GetRecentGames((int) summoner.AccountId);
            var set = new HashSet<long>();

            foreach (var game in recent.gameStatistics)
            {
                foundGame(game);

                foreach (var participant in game.fellowPlayers)
                {
                    if (set.Contains(participant.summonerId)) continue;
                    
                    set.Add(participant.summonerId);
                    foundSummoner(participant);
                }
            }
        }

        private async Task CrawlGames(long id)
        {
            var recent = await _statsService.GetRecentGames((int)id);

            var set = new HashSet<long>();
            foreach (var game in recent.gameStatistics)
            {
                OnGameCrawled(game);

                //var data = new Game {Id = game.id.ToString()};
                foreach (var participant in game.fellowPlayers)
                {
//                    var participantData = new Participant
//                    {
//                        SummonerId = participant.summonerId,
//                        ChampionId = participant.championId
//                    };
//
//                    if (participant.teamId == 100)
//                        data.TeamOne.Add(participantData);
//                    else
//                        data.TeamTwo.Add(participantData);

                    if (set.Contains(participant.summonerId)) 
                        continue;

                    set.Add(participant.summonerId);
                    OnSummonerCrawled(participant);
                }
            }
        }
    }
}