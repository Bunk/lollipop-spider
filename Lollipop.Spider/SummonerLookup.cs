using System;
using System.Globalization;
using System.Threading.Tasks;
using log4net;
using Lollipop.Services;
using Lollipop.Spider.Data.Domain;

namespace Lollipop.Spider
{
    public class SummonerLookup : ILookupSummoners
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (SummonerLookup));

        private readonly ISummonerService _summonerService;

        public SummonerLookup(ISummonerService summonerService)
        {
            _summonerService = summonerService;
        }

        public async Task<Summoner> Hydrate(long id)
        {
            try
            {
                var summoner = await _summonerService.PublicData((int)id);
                var named = await _summonerService.Get(summoner.summoner.name);
                //var kudos = await _summonerService.Kudos((int) id);
                //var stats = await _statsService.GetLifetimeStats((int) id);
                //var stats2 = await _statsService.GetAggregatedStats((int) id, GameMode.CLASSIC);
                //var champs = await _statsService.GetMostPlayedChamps((int) id, GameMode.CLASSIC);

                return new Summoner
                {
                    Id = summoner.summoner.sumId.ToString(CultureInfo.InvariantCulture),
                    AccountId = summoner.summoner.acctId,
                    ProfileIconId = summoner.summoner.profileIconId,
                    Name = summoner.summoner.name,
                    InternalName = summoner.summoner.internalName,
                    Level = summoner.summonerLevel.summonerLevel,
                    RevisionDate = named.revisionDate,
                    LastCrawledDate = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                Log.Warn("An error occurred while trying to hydrate a summoner by its Id.", ex);
                return null;
            }
        }

        public async Task<Summoner> Lookup(long id)
        {
            var name = await _summonerService.NamesById((int)id);
            var acct = await _summonerService.Get(name[0]);

            return new Summoner
            {
                Id = id.ToString(CultureInfo.InvariantCulture),
                AccountId = acct.acctId,
                ProfileIconId = acct.profileIconId,
                Name = acct.name,
                InternalName = acct.internalName,
                RevisionDate = acct.revisionDate,
                Level = acct.summonerLevel
            };
        }
    }
}