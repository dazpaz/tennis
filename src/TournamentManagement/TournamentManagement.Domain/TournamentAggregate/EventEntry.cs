﻿using DomainDesign.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TournamentManagement.Domain.Common;
using TournamentManagement.Domain.PlayerAggregate;

namespace TournamentManagement.Domain.TournamentAggregate
{
	public class EventEntry : Entity<EventEntryId>
	{
		public EventId EventId { get; private set; }
		public EventType EventType { get; private set; }
		public IReadOnlyCollection<Player> Players { get; private set; }
		public ushort Rank { get; private set; }

		private readonly IList<Player> _players;

		private EventEntry(EventEntryId id) : base(id)
		{
			_players = new List<Player>();
			Players = new ReadOnlyCollection<Player>(_players);
		}

		public static EventEntry CreateSinglesEventEntry(EventId eventId, EventType eventType,
			Player player)
		{
			GuardAgainstDoublesEvent(eventType);
			GuardAgainstWrongGenderForEventType(eventType, new List<Player> { player });

			var entry = CreateEntry(eventId, eventType);
			entry._players.Add(player);
			entry.Rank = player.SinglesRank;
			return entry;
		}

		public static EventEntry CreateDoublesEventEntry(EventId eventId, EventType eventType,
			Player playerOne, Player playerTwo)
		{
			GuardAgainstSinglesEvent(eventType);
			GuardAgainstWrongGenderForEventType(eventType, new List<Player> { playerOne, playerTwo });

			var entry = CreateEntry(eventId, eventType);
			entry._players.Add(playerOne);
			entry._players.Add(playerTwo);
			entry.Rank = entry._players.Min(p => p.DoublesRank);
			return entry;
		}

		private static EventEntry CreateEntry(EventId eventId, EventType eventType)
		{
			var entry = new EventEntry(new EventEntryId())
			{
				EventId = new EventId(eventId.Id),
				EventType = eventType,
			};

			return entry;
		}

		private static void GuardAgainstSinglesEvent(EventType eventType)
		{
			if (Event.IsSinglesEvent(eventType))
			{
				throw new ArgumentException($"{eventType} is not a doubles event");
			}
		}

		private static void GuardAgainstDoublesEvent(EventType eventType)
		{
			if (!Event.IsSinglesEvent(eventType))
			{
				throw new ArgumentException($"{eventType} is not a singles event");
			}
		}

		private static void GuardAgainstWrongGenderForEventType(EventType eventtype, IEnumerable<Player> players)
		{
			var maleCount = players.Count(p => p.Gender == Gender.Male);
			var femaleCount = players.Count(p => p.Gender == Gender.Female);

			if (!EventGenderValidator.IsValid(eventtype, maleCount, femaleCount))
			{
				throw new Exception($"Gender of players does not match the event type {eventtype}");
			}
		}
	}
}