﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AoCStats
{
    [DebuggerDisplay("{Name}: {LocalScore}")]
    public class Player
    {
        public Player()
        {
            const int dayCount = 25;
            unixCompletionTime = new long[dayCount][];
            TimeToComplete = new TimeSpan?[dayCount][];
            AccumulatedTimeToComplete = new TimeSpan?[dayCount][];
            OffsetFromWinner = new TimeSpan?[dayCount][];
            TimeToCompleteStar2 = new TimeSpan?[dayCount];
            PositionForStar = new int[dayCount][];
            AccumulatedScore = new int[dayCount][];
            AccumulatedPosition = new int[dayCount][];
            GlobalScoreForDay = new int?[dayCount][];
            for (int i = 0; i < dayCount; i++)
            {
                unixCompletionTime[i] = new long[] { -1, -1 };
                PositionForStar[i] = new[] { -1, -1 };
                AccumulatedScore[i] = new[] { -1, -1 };
                AccumulatedPosition[i] = new[] { -1, -1 };
                GlobalScoreForDay[i] = new int?[] { null, null };
                TimeToComplete[i] = new TimeSpan?[2];
                AccumulatedTimeToComplete[i] = new TimeSpan?[2];
                TimeToCompleteStar2[i] = null;
                OffsetFromWinner[i] = new TimeSpan?[2];
            }
        }

        public int?[][] GlobalScoreForDay { get; set; }

        public int TotalScore { get; set; }
        public int[][] PositionForStar { get; set; }
        public int[][] AccumulatedScore { get; set; }
        public int[][] AccumulatedPosition { get; set; }

        public string Name { get; set; }
        public long LastStar { get; set; }
        public int Stars { get; set; }
        public int LocalScore { get; set; }
        public int GlobalScore { get; set; }
        public string Id { get; set; }
        public long[][] unixCompletionTime { get; }
        public TimeSpan?[][] TimeToComplete { get; set; }
        public TimeSpan?[][] AccumulatedTimeToComplete { get; set; }
        public TimeSpan?[][] OffsetFromWinner { get; set; }
        public TimeSpan?[] TimeToCompleteStar2 { get; set; }
        public string Props { get; set; }
        public int CurrentPosition { get; set; }
    }

    public class LeaderBoard
    {
        public LeaderBoard(List<Player> players, int highestDay)
        {
            Players = players;
            HighestDay = highestDay;
            TopScorePerDay = new int[highestDay][];
            for (int i = 0; i < highestDay; i++)
                TopScorePerDay[i] = new int[2];
        }

        public List<Player> Players { get; }
        public int HighestDay { get; }
        public int[][] TopScorePerDay { get; }
    }
}