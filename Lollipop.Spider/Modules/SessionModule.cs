using Lollipop.Services;
using Lollipop.Session;
using Lollipop.Spider.Workflow;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;

namespace Lollipop.Spider.Modules
{
    public class SessionModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ILocateServerIP>().To<LocateServerIP>().InSingletonScope();
            Bind<IAuthorize>().To<AuthorizationService>().InSingletonScope();

            Bind<ILeagueAccount>().ToMethod(BuildAccount).InSingletonScope();
            Bind<ILeagueConnection>().To<LeagueConnection>().InTransientScope();
            Bind<IFlashRemotingClient>().To<LeagueClient>().InTransientScope();

            Bind<IStatsService>().To<StatsService>().InTransientScope();
            Bind<IGameService>().To<GameService>().InTransientScope();
            Bind<ISummonerService>().To<SummonerService>().InTransientScope();

            Bind<IProduceSummoners>().To<SummonerProducer>().InTransientScope();
            Bind<ILookupSummoners>().To<SummonerLookup>().InTransientScope();
            Bind<ICrawlSummoners>().To<SummonerCrawler>().InTransientScope();
            Bind<IStoreSummoners>().To<SummonerProducer>().InTransientScope();

            Bind<SummonerWorkflow>().ToSelf().InTransientScope();
        }

        private static ILeagueAccount BuildAccount(IContext arg)
        {
            var account = new CompositeLeagueAccount()
                .AddAccount(new LeagueAccount(arg.Kernel.Get<IFlashRemotingClient>(),
                                              LeagueRegion.NorthAmerica, "BunkTester", "leaguetester1"))
                .AddAccount(new LeagueAccount(arg.Kernel.Get<IFlashRemotingClient>(),
                                              LeagueRegion.NorthAmerica, "BunkTester2", "leaguetester2"));

            var errors = account.ConnectAll();

            return account;
        }
    }
}
