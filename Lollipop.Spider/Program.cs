using System;
using System.Globalization;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lollipop.Services;
using Lollipop.Session;
using Lollipop.Spider.Data.Domain;
using Lollipop.Spider.Modules;
using Lollipop.Spider.Reactive;
using LoveSeat;
using Ninject;

namespace Lollipop.Spider
{
    public class Program
    {
        private static ILeagueAccount _account;

        public static void Main(string[] args)
        {
            var kernel = new StandardKernel(new SessionModule(),
                                            new DataModule());

            var producer = new SummonerProducer(kernel.Get<CouchClient>());
            var lookup = new SummonerLookup(kernel.Get<ISummonerService>());
            var poller = new SummonerPollingAgent(producer);
            var crawler = new SummonerCrawler(producer, kernel.Get<ISummonerService>(), kernel.Get<IStatsService>());

            var summoners = poller.BeginPolling()
                .SelectMany(crawler.Crawl)
                .SelectMany(x => x)
                .Select(lookup.Lookup)
                .Select(async summoner =>
                {
                    var data = await summoner;
                    producer.Store(data);
                    return data;
                });

            using (summoners.Subscribe(
                async id => Console.WriteLine("Id: {0}", await id),
                err => Console.WriteLine("ERROR: {0}", err),
                () => Console.WriteLine("Done with current poll!")))
            {
                Console.WriteLine("Press any key to unsubscribe!");
                Console.ReadKey();
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
