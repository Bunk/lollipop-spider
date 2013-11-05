using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lollipop.Session;
using Lollipop.Spider.Modules;
using Ninject;
using Ninject.Activation;

namespace Lollipop.Spider
{
    public class Program
    {
        private static ILeagueAccount _account;

        public static void Main(string[] args)
        {
            var kernel = new StandardKernel(new SessionModule());

            var account = new CompositeLeagueAccount()
                .AddAccount(new LeagueAccount(kernel.Get<IFlashRemotingClient>(),
                                              LeagueRegion.NorthAmerica, "BunkTester", "leaguetester1"))
                .AddAccount(new LeagueAccount(kernel.Get<IFlashRemotingClient>(),
                                              LeagueRegion.NorthAmerica, "BunkTester2", "leaguetester2"));

            var errors = account.ConnectAll();

            _account = account;

            // Pull current listing of games
            // Get all summoners participating in those games

            // Push summoner ids into the queue of summoners to query

            // On a separate thread, go through all summoners in the queue
            // Get all public data for those summoners
            // Get the past ten games for those summoners
            // For each game, place each participant into the queue
            // Record the end of game statistics

            // Repeat by reviewing the queue

            // On a separate worker, periodically scan the queue to bump stale summoners
        }
    }
}
