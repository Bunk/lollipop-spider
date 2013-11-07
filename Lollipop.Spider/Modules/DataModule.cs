using LoveSeat;
using Ninject.Modules;

namespace Lollipop.Spider.Modules
{
    public class DataModule : NinjectModule
    {
        public override void Load()
        {
            Bind<CouchClient>().ToMethod(c => new CouchClient("admin", "party"));
        }
    }
}
