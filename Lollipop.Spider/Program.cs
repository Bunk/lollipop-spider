using System;
using System.Threading;
using Lollipop.Spider.Modules;
using Lollipop.Spider.Workflow;
using Ninject;

namespace Lollipop.Spider
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var kernel = new StandardKernel(new SessionModule(), new DataModule());

            using (var workflow = kernel.Get<SummonerWorkflow>())
            using (var cancel = new CancellationTokenSource())
            {
                workflow.Execute(cancel.Token);

                Console.WriteLine("Press any key to stop running...");
                Console.ReadKey();

                cancel.Cancel();
            }

//            using (poller.BeginPolling().Subscribe(new SummonerObserver()))
//            {
//                Console.WriteLine("Subscribed!");
//                Console.WriteLine("Press any key to unsubscribe!");
//                Console.ReadKey();
//            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}