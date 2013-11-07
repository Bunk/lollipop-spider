using System;
using System.Collections.Generic;
using System.Security;

namespace Lollipop.Spider.Data.Domain
{
    public class Game
    {
        public string Id { get; set; }

        public List<Participant> TeamOne { get; set; }

        public List<Participant> TeamTwo { get; set; }

        public Game()
        {
            TeamOne = new List<Participant>();
            TeamTwo = new List<Participant>();
        }
    }

    public class Participant
    {
        public long ChampionId { get; set; }

        public long SummonerId { get; set; }
    }

    public class Summoner
    {
        public long Id { get; set; }

        public long AccountId { get; set; }

        public int ProfileIconId { get; set; }

        public int Level { get; set; }

        public string Name { get; set; }

        public string InternalName { get; set; }

        public string SeasonOneTier { get; set; }

        public string SeasonTwoTier { get; set; }

        public string SeasonThreeTier { get; set; }

        public DateTime LastGameDate { get; set; }

        public DateTime RevisionDate { get; set; }

        public DateTime? LastCrawledDate { get; set; }

        public DateTime? PreviousFirstWinOfDay { get; set; }

        public DateTime? PromotionGamesPlayedUpdatedDate { get; set; }

        public int PromotionGamesPlayed { get; set; }

        public LeaverPenalties LeaverPenalties { get; set; }
    }

    public class LeaverPenalties
    {
        public bool UserInformed { get; set; }

        public int Level { get; set; }

        public int Points { get; set; }

        public DateTime? LastDecayDate { get; set; }

        public DateTime? LastLevelIncreaseDate { get; set; }

        public DateTime? DodgePenaltyDate { get; set; }
    }

    public class SeasonSummonerStats
    {
        public int Season { get; set; }

        public Dictionary<string, SeasonSummonerStatSummary> Stats { get; set; }

        public SeasonSummonerStats()
        {
            Stats = new Dictionary<string, SeasonSummonerStatSummary>();
        }
    }

    public class SeasonSummonerStatSummary
    {
        public string Type { get; set; }

        public int TotalLeaves { get; set; }

        public int TotalLosses { get; set; }

        public int TotalWins { get; set; }

        public int Rating { get; set; }

        public int MaxRating { get; set; }

        public Dictionary<string, AggregatedSummaryStat> AggregatedStats { get; set; }

        public SeasonSummonerStatSummary()
        {
            AggregatedStats = new Dictionary<string, AggregatedSummaryStat>();
        }
    }

    public class AggregatedSummaryStat
    {
        public string Type { get; set; }

        public double Value { get; set; }

        public double Count { get; set; }
    }
}
