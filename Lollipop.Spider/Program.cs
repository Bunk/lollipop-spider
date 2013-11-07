using System;
using System.Threading;
using Lollipop.Session;
using Lollipop.Spider.Modules;
using Lollipop.Spider.Workers;
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

            var handler = kernel.Get<SummonerProducer>();

            using (var cancellation = new CancellationTokenSource())
            {
                handler.Produce(cancellation.Token);

                Console.WriteLine("Press any key to exit.");
                Console.ReadLine();

                cancellation.Cancel();
            }
        }
    }
}
