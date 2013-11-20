using System;

namespace Lollipop.Spider.Reactive
{
    public interface IPollSummoners
    {
        int Freshness { get; set; }
        int MaxPollingFrequencyInSeconds { get; set; }
        int MaxResults { get; set; }
        IObservable<long> BeginPolling();
    }
}