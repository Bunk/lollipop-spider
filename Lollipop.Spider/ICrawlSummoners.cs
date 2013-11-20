using System;
using System.Threading.Tasks;
using com.riotgames.platform.statistics;
using Lollipop.Spider.Data.Domain;

namespace Lollipop.Spider
{
    public interface ICrawlSummoners
    {
        Task Crawl(Summoner summoner, Action<FellowPlayerInfo> foundSummoner, Action<PlayerGameStats> foundGame);
    }
}