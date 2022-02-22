﻿using System;

namespace TournamentManagement.Contract
{
	public class TournamentSummaryDto
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
		public TournamentLevel TournamentLevel { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string VenueName { get; set; }
	}
}
