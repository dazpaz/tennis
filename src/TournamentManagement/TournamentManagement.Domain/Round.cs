﻿using DomainDesign.Common;

namespace TournamentManagement.Domain
{
	public class Round : Entity<RoundId>
	{
		private static readonly int[] AllowedCompetitorCount = { 128, 64, 32, 16, 8, 4 ,2 };

		public TournamentId TournamentId { get; private set; }
		public EventType EventType { get; private set; }
		public int RoundNumber { get; private set; }
		public string Title { get; private set; }
		public int CompetitorCount { get; private set; }

		private Round(RoundId id) : base(id)
		{
		}
		
		public static Round Create(TournamentId tournamentId, EventType eventType,
			int roundNumber, int competitorCount)
		{
			Guard.ForIntegerOutOfRange(roundNumber, 1, 7, "roundNumber");
			Guard.ForValueNotInSetOfAllowedValues(competitorCount, AllowedCompetitorCount, "playerCount");

			var round = new Round(new RoundId())
			{
				TournamentId = tournamentId,
				EventType = eventType,
				RoundNumber = roundNumber,
				Title = GetRoundTitle(competitorCount),
				CompetitorCount = competitorCount
			};

			return round;
		}

		private static string GetRoundTitle(int playerCount)
		{
			if (playerCount == 2) return "Final";
			if (playerCount == 4) return "Semi-Final";
			if (playerCount == 8) return "Quarter-Final";
			return $"Round of {playerCount}";
		}
	}
}