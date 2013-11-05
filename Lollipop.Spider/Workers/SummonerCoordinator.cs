using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Lollipop.Services;

namespace Lollipop.Spider.Workers
{
    public class SummonerCoordinator
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly Func<ActiveChampionCrawler> _champCrawlerFactory;

        public SummonerCoordinator(Func<ActiveChampionCrawler> champCrawlerFactory)
        {
            _champCrawlerFactory = champCrawlerFactory;
        }

        public async Task Execute()
        {
            var nextIds = PullNextBlock();
            while (nextIds.Count > 0)
            {
                await Crawl(nextIds);
                nextIds = PullNextBlock();
            }
        }

        private List<long> PullNextBlock()
        {
            // we do this within a transaction to ensure that we're consistent
            using (new TransactionScope())
            {
                // get the next block of ids that meet the conditions
                var ids = new List<long>();

                // batch update the list with the current date time

                // save these records so we don't pull them the next time

                // return the next block
                return ids;
            }
        }

        private async Task Crawl(List<long> idBlock)
        {
            await _semaphore.WaitAsync();
            try
            {
                var found = await _champCrawlerFactory().Crawl(idBlock);
                Process(found);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void Process(IEnumerable<SummonerGame> summonerGames)
        {
            foreach (var game in summonerGames)
            {
                using (new TransactionScope())
                {
                    // Upsert the summoner queue id blocks
                    // Insert new data if the summoner id doesn't exist
                    // Update latest activity date with the date if it's later
                }
            }
        }
    }
}
