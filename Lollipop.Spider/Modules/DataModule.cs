using LoveSeat;
using Ninject.Modules;

namespace Lollipop.Spider.Modules
{
    public class DataModule : NinjectModule
    {
        public override void Load()
        {
            Bind<CouchClient>()
                .ToMethod(c => new CouchClient("192.168.1.70", 5984, "admin", "party", false, AuthenticationType.Basic));
        }
    }
}
