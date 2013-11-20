using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Lollipop.Spider.Reactive
{
    public class SummonerObserver : IObserver<long>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SummonerObserver));

        private readonly ICrawlSummoners _crawler;
        private readonly IStoreSummoners _storeSummoners;
        private readonly ILookupSummoners _lookupSummoner;

        public SummonerObserver(ICrawlSummoners crawler, IStoreSummoners storeSummoners, ILookupSummoners lookupSummoner)
        {
            _crawler = crawler;
            _storeSummoners = storeSummoners;
            _lookupSummoner = lookupSummoner;
        }

        public void OnNext(long value)
        {
            //var task = _crawler.Crawl(value);
            //task.Wait();
            
//            foreach (var id in task.Result)
//            {
//                var lookupTask = _lookupSummoner.Lookup(id);
//                lookupTask.Wait();
//                
//                _storeSummoners.Store(lookupTask.Result);
//            }
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }
    }
}
