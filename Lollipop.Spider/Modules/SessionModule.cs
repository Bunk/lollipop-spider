using Lollipop.Session;
using Ninject.Modules;

namespace Lollipop.Spider.Modules
{
    public class SessionModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ILocateServerIP>().To<LocateServerIP>().InSingletonScope();
            Bind<IAuthorize>().To<AuthorizationService>().InSingletonScope();

            Bind<ILeagueConnection>().To<LeagueConnection>().InTransientScope();
            Bind<IFlashRemotingClient>().To<LeagueClient>().InTransientScope();


        }
    }
}
