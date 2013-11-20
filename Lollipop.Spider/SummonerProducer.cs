using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Lollipop.Spider.Data.Domain;
using LoveSeat;

namespace Lollipop.Spider
{
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
                return Enumerable.Empty<long>();
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
            var existing = _summoners.GetDocument<Summoner>(summoner.Id);
            if (existing != null)
                existing = UpdateExisting(existing, summoner);

            var result = _summoners.SaveDocument(new Document<Summoner>(existing));
        }

        public void StoreWhenMissing(Summoner summoner)
        {
            var existing = _summoners.GetDocument(summoner.Id);
            if (existing != null)
                return;

            var result = _summoners.SaveDocument(new Document<Summoner>(summoner));
        }

        private Summoner UpdateExisting(Summoner existing, Summoner summoner)
        {
            summoner.Rev = existing.Rev;
//            existing.AccountId = summoner.AccountId;
//            existing.InternalName = summoner.InternalName;
//            existing.LastCrawledDate = summoner.LastCrawledDate;
//            existing.LastGameDate = summoner.LastGameDate;
//            existing.LeaverPenalties = summoner.LeaverPenalties;
//            existing.Level = summoner.Level;
//            existing.Name = summoner.Name;
//            existing.PreviousFirstWinOfDay = summoner.PreviousFirstWinOfDay;
//            existing.ProfileIconId = summoner.ProfileIconId;
//            existing.PromotionGamesPlayed = summoner.PromotionGamesPlayed;
//            existing.PromotionGamesPlayedUpdatedDate = summoner.PromotionGamesPlayedUpdatedDate;
//            existing.RevisionDate = summoner.RevisionDate;
//            existing.SeasonOneTier = summoner.SeasonOneTier;
//            existing.SeasonTwoTier = summoner.SeasonTwoTier;
//            existing.SeasonThreeTier = summoner.SeasonThreeTier;
            return summoner;
        }
    }
}