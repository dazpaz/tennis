﻿using FluentAssertions;
using System;
using TournamentManagement.Common;
using TournamentManagement.Domain.Common;
using TournamentManagement.Domain.PlayerAggregate;
using TournamentManagement.Domain.TournamentAggregate;
using Xunit;

namespace TournamentManagement.Domain.UnitTests.TournamentAggregate
{
	public class EventTests
	{
		[Fact]
		public void CanCreateAnEventUsingTheFactory()
		{
			var tennisEvent = Event.Create(EventType.MensSingles, 128, 32,
				MatchFormat.ThreeSetMatchWithFinalSetTieBreak);

			tennisEvent.Id.Id.Should().NotBe(Guid.Empty);
			tennisEvent.IsCompleted.Should().BeFalse();
			tennisEvent.EventType.Should().Be(EventType.MensSingles);
			tennisEvent.MatchFormat.Should().Be(MatchFormat.ThreeSetMatchWithFinalSetTieBreak);
			tennisEvent.EventSize.EntrantsLimit.Should().Be(128);
			tennisEvent.EventSize.NumberOfSeeds.Should().Be(32);
		}

		[Theory]
		[InlineData(EventType.MensDoubles, false)]
		[InlineData(EventType.MensSingles, true)]
		[InlineData(EventType.MixedDoubles, false)]
		[InlineData(EventType.WomensDoubles, false)]
		[InlineData(EventType.WomensSingles, true)]
		public void TheIsSinglesEventPropertyIsSetCorrectlyBasedOnEventType(EventType eventType, bool isSingles)
		{
			var tennisEvent = Event.Create(eventType, 128, 32, new MatchFormat(1, SetType.TieBreak));

			tennisEvent.SinglesEvent.Should().Be(isSingles);
		}

		[Fact]
		public void CanUpdateAnEvent()
		{
			var tennisEvent = Event.Create(EventType.MensSingles, 128, 32,
				MatchFormat.ThreeSetMatchWithFinalSetTieBreak);

			var id = tennisEvent.Id;

			tennisEvent.UpdateDetails(EventType.MensDoubles, 64, 16,
				MatchFormat.OneSetMatchWithFinalSetTieBreak);

			tennisEvent.Id.Should().Be(id);
			tennisEvent.EventType.Should().Be(EventType.MensDoubles);
			tennisEvent.MatchFormat.Should().Be(MatchFormat.OneSetMatchWithFinalSetTieBreak);
			tennisEvent.EventSize.EntrantsLimit.Should().Be(64);
			tennisEvent.EventSize.NumberOfSeeds.Should().Be(16);
		}

		[Fact]
		public void CanMarkAnEventAsCompleted()
		{
			var tennisEvent = Event.Create(EventType.MensSingles, 128, 32,
				MatchFormat.ThreeSetMatchWithFinalSetTieBreak);

			tennisEvent.IsCompleted.Should().BeFalse();

			tennisEvent.CompleteEvent();

			tennisEvent.IsCompleted.Should().BeTrue();
		}

		[Fact]
		public void CannotUpdateAnEventThatIsCompleted()
		{
			var tennisEvent = Event.Create(EventType.MensSingles, 128, 32,
				MatchFormat.ThreeSetMatchWithFinalSetTieBreak);
			tennisEvent.CompleteEvent();

			Action act = () => tennisEvent.UpdateDetails(EventType.WomensSingles, 64, 16,
				MatchFormat.ThreeSetMatchWithFinalSetTieBreak);

			act.Should()
				.Throw<Exception>()
				.WithMessage("Cannot update the details of an event that is completed");
		}

		[Fact]
		public void PlayerCanEnterASinglesEvent()
		{
			var tennisEvent = Event.Create(EventType.MensSingles, 128, 32,
				MatchFormat.ThreeSetMatchWithFinalSetTieBreak);
			var player = Player.Register(new PlayerId(), "Steve", 100, 50, Gender.Male);

			tennisEvent.EnterEvent(player);

			tennisEvent.Entries.Count.Should().Be(1);
			tennisEvent.Entries[0].PlayerOne.Should().Be(player);
			tennisEvent.Entries[0].PlayerTwo.Should().BeNull();
		}

		[Fact]
		public void PairOfPlayersCanEnterADoublesEvent()
		{
			var tennisEvent = Event.Create(EventType.MensDoubles, 128, 32,
				MatchFormat.ThreeSetMatchWithFinalSetTieBreak);
			var playerOne = Player.Register(new PlayerId(), "Steve", 100, 50, Gender.Male);
			var playerTwo = Player.Register(new PlayerId(), "Dave", 20, 150, Gender.Male);

			tennisEvent.EnterEvent(playerOne, playerTwo);

			tennisEvent.Entries.Count.Should().Be(1);
			tennisEvent.Entries[0].PlayerOne.Should().Be(playerOne);
			tennisEvent.Entries[0].PlayerTwo.Should().Be(playerTwo);
		}

		[Fact]
		public void CannotEnterSinglesEventIfPlayerIsNull()
		{
			var tennisEvent = Event.Create(EventType.MensSingles, 128, 32,
				MatchFormat.ThreeSetMatchWithFinalSetTieBreak);

			Action act = () => tennisEvent.EnterEvent(null);

			act.Should().Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null. (Parameter 'playerOne')");
		}

		[Fact]
		public void CannotEnterDoublesEventIfEitherPlayerIsNull()
		{
			var tennisEvent = Event.Create(EventType.MensDoubles, 128, 32,
				MatchFormat.ThreeSetMatchWithFinalSetTieBreak);

			var player = Player.Register(new PlayerId(), "Steve", 100, 50, Gender.Male);

			Action act = () => tennisEvent.EnterEvent(null, player);

			act.Should().Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null. (Parameter 'playerOne')");

			act = () => tennisEvent.EnterEvent(player, null);

			act.Should().Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null. (Parameter 'playerTwo')");
		}

		[Fact]
		public void CannotEnterSinglesEventIfPlayerIsAlreadyEnteredInTheEvent()
		{
			var tennisEvent = Event.Create(EventType.MensSingles, 128, 32,
				MatchFormat.ThreeSetMatchWithFinalSetTieBreak);

			var player = Player.Register(new PlayerId(), "Steve", 100, 50, Gender.Male);

			tennisEvent.EnterEvent(Player.Register(new PlayerId(), "Dave", 100, 50, Gender.Male));
			tennisEvent.EnterEvent(Player.Register(new PlayerId(), "Peter", 100, 50, Gender.Male));
			tennisEvent.EnterEvent(player);

			tennisEvent.Entries.Count.Should().Be(3);

			Action act = () => tennisEvent.EnterEvent(player);

			act.Should().Throw<Exception>()
				.WithMessage("Player Steve has already entered this event");
		}

		[Fact]
		public void CannotEnterDoublesEventIfPlayerOneIsAlreadyEnteredInTheEvent()
		{
			var tennisEvent = Event.Create(EventType.MensDoubles, 128, 32,
				MatchFormat.ThreeSetMatchWithFinalSetTieBreak);

			var player = Player.Register(new PlayerId(), "Steve", 100, 50, Gender.Male);

			tennisEvent.EnterEvent(Player.Register(new PlayerId(), "Dave", 100, 50, Gender.Male),
				Player.Register(new PlayerId(), "Peter", 100, 50, Gender.Male));

			tennisEvent.EnterEvent(Player.Register(new PlayerId(), "John", 100, 50, Gender.Male),
				Player.Register(new PlayerId(), "Lee", 100, 50, Gender.Male));

			tennisEvent.EnterEvent(Player.Register(new PlayerId(), "Barry", 100, 50, Gender.Male), player);

			tennisEvent.Entries.Count.Should().Be(3);

			Action act = () => tennisEvent.EnterEvent(player,
				Player.Register(new PlayerId(), "Chris", 100, 50, Gender.Male));

			act.Should().Throw<Exception>()
				.WithMessage("Player Steve has already entered this event");
		}

		[Fact]
		public void CannotEnterDoublesEventIfPlayerTwoIsAlreadyEnteredInTheEvent()
		{
			var tennisEvent = Event.Create(EventType.MensDoubles, 128, 32,
				MatchFormat.ThreeSetMatchWithFinalSetTieBreak);

			var player = Player.Register(new PlayerId(), "Steve", 100, 50, Gender.Male);

			tennisEvent.EnterEvent(Player.Register(new PlayerId(), "Dave", 100, 50, Gender.Male),
				Player.Register(new PlayerId(), "Peter", 100, 50, Gender.Male));

			tennisEvent.EnterEvent(Player.Register(new PlayerId(), "John", 100, 50, Gender.Male),
				Player.Register(new PlayerId(), "Lee", 100, 50, Gender.Male));

			tennisEvent.EnterEvent(player, Player.Register(new PlayerId(), "Barry", 100, 50, Gender.Male));

			tennisEvent.Entries.Count.Should().Be(3);

			Action act = () => tennisEvent.EnterEvent(Player.Register(new PlayerId(), "Chris", 100, 50, Gender.Male),
				player);

			act.Should().Throw<Exception>()
				.WithMessage("Player Steve has already entered this event");
		}

		[Fact]
		public void PlayerCanWithdrawFromSinglesEvent()
		{
			var tennisEvent = Event.Create(EventType.MensSingles, 128, 32,
				MatchFormat.ThreeSetMatchWithFinalSetTieBreak);

			var player = Player.Register(new PlayerId(), "Steve", 100, 50, Gender.Male);

			tennisEvent.EnterEvent(Player.Register(new PlayerId(), "Dave", 100, 50, Gender.Male));
			tennisEvent.EnterEvent(Player.Register(new PlayerId(), "Peter", 100, 50, Gender.Male));
			tennisEvent.EnterEvent(player);

			tennisEvent.Entries.Count.Should().Be(3);

			tennisEvent.WithdrawFromEvent(player);

			tennisEvent.Entries.Count.Should().Be(2);
		}

		[Fact]
		public void PairOfPlayersCanWithdrawFromDoublesEvent()
		{
			var tennisEvent = Event.Create(EventType.MensDoubles, 128, 32,
				MatchFormat.ThreeSetMatchWithFinalSetTieBreak);
			var playerOne = Player.Register(new PlayerId(), "Steve", 100, 50, Gender.Male);
			var playerTwo = Player.Register(new PlayerId(), "Dave", 20, 150, Gender.Male);

			tennisEvent.EnterEvent(Player.Register(new PlayerId(), "John", 100, 50, Gender.Male),
				Player.Register(new PlayerId(), "Lee", 100, 50, Gender.Male));
			tennisEvent.EnterEvent(playerOne, playerTwo);

			tennisEvent.Entries.Count.Should().Be(2);

			tennisEvent.WithdrawFromEvent(playerOne, playerTwo);

			tennisEvent.Entries.Count.Should().Be(1);
		}

		[Fact]
		public void CannotWithdrawFromSinglesEventIfPlayerIsNotSpecified()
		{
			var tennisEvent = Event.Create(EventType.MensSingles, 128, 32,
				MatchFormat.ThreeSetMatchWithFinalSetTieBreak);

			Action act = () => tennisEvent.WithdrawFromEvent(null);

			act.Should().Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null. (Parameter 'playerOne')");
		}

		[Fact]
		public void CannotWithdrawFromDoublesEventIfPlayerOneIsNotSpecified()
		{
			var tennisEvent = Event.Create(EventType.MensDoubles, 128, 32,
				MatchFormat.ThreeSetMatchWithFinalSetTieBreak);

			Action act = () => tennisEvent.WithdrawFromEvent(null, null);

			act.Should().Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null. (Parameter 'playerOne')");
		}

		[Fact]
		public void CannotWithdrawFromDoublesEventIfPlayerTwoIsNotSpecified()
		{
			var tennisEvent = Event.Create(EventType.MensDoubles, 128, 32,
				MatchFormat.ThreeSetMatchWithFinalSetTieBreak);
			var playerOne = Player.Register(new PlayerId(), "John", 2, 4, Gender.Male);

			Action act = () => tennisEvent.WithdrawFromEvent(playerOne, null);

			act.Should().Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null. (Parameter 'playerTwo')");
		}

		[Fact]
		public void CannotWithdrawFromSinglesEventIfPlayerWasNotEntered()
		{
			var tennisEvent = Event.Create(EventType.MensSingles, 128, 32,
				MatchFormat.ThreeSetMatchWithFinalSetTieBreak);

			var player = Player.Register(new PlayerId(), "Steve", 100, 50, Gender.Male);

			tennisEvent.EnterEvent(Player.Register(new PlayerId(), "Dave", 100, 50, Gender.Male));
			tennisEvent.EnterEvent(Player.Register(new PlayerId(), "Peter", 100, 50, Gender.Male));

			Action act = () => tennisEvent.WithdrawFromEvent(player);

			act.Should().Throw<Exception>()
				.WithMessage("Player was not entered into the event");
		}

		[Fact]
		public void CannotWithdrawFromDoublesEventIfPlayersWereNotEntered()
		{
			var tennisEvent = Event.Create(EventType.MensDoubles, 128, 32,
				MatchFormat.ThreeSetMatchWithFinalSetTieBreak);

			var playerOne = Player.Register(new PlayerId(), "Steve", 100, 50, Gender.Male);
			var playerTwo = Player.Register(new PlayerId(), "Boris", 10, 55, Gender.Male);

			tennisEvent.EnterEvent(Player.Register(new PlayerId(), "Dave", 100, 50, Gender.Male),
				Player.Register(new PlayerId(), "Peter", 100, 50, Gender.Male));
			tennisEvent.EnterEvent(playerOne, Player.Register(new PlayerId(), "Lee", 100, 50, Gender.Male));
			tennisEvent.EnterEvent(Player.Register(new PlayerId(), "John", 100, 50, Gender.Male), playerTwo);

			Action act = () => tennisEvent.WithdrawFromEvent(playerOne, playerTwo);

			act.Should().Throw<Exception>()
				.WithMessage("Players were not entered into the event");
		}
	}
}
