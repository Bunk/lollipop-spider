using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Lollipop.Spider.Reactive
{
    public class SummonerPollingAgent : IPollSummoners
    {
        public int Freshness { get; set; }

        public int MaxPollingFrequencyInSeconds { get; set; }

        public int MaxResults { get; set; }

        private readonly IProduceSummoners _summonerProducer;

        public SummonerPollingAgent(IProduceSummoners summonerProducer)
        {
            Freshness = (int) TimeSpan.FromDays(1).TotalMinutes;
            MaxPollingFrequencyInSeconds = 30;
            MaxResults = 30;

            _summonerProducer = summonerProducer;
        } 

        public IObservable<long> BeginPolling()
        {
            var frequency = TimeSpan.FromSeconds(MaxPollingFrequencyInSeconds);

            return Observable
                .Create<long>(observer =>
                    NewThreadScheduler
                        .Default
                        .SchedulePeriodic(frequency, PullData(observer, Freshness, MaxResults)));
        }

        private Action PullData(IObserver<long> observer, int freshnessFilter, int maxResults)
        {
            return () =>
            {
                var found = _summonerProducer.Produce(freshnessFilter, maxResults);
                foreach (var item in found)
                    observer.OnNext(item);
            };
        }
    }
}