using System.Collections.Generic;

namespace UscParser.Models
{
	public class EventSummaryDto
	{
		public string Event { get; set; }
		public string Location { get; set; }
		public string EventDates { get; set; }
		public string SponsoringAffiliates { get; set; }
		public string ChiefTournamentDirector { get; set; }
		public string ChiefAssistantTournamentDirector { get; set; }
		public string OtherTournamentDirectors { get; set; }
		public string Processed { get; set; }
		public string Stats { get; set; }
	}

	public class EventSummary
	{
		public Entity Event { get; set; }
		public string Location { get; set; }
		public string EventDates { get; set; }
		public Entity SponsoringAffiliates { get; set; }
		public Entity ChiefTournamentDirector { get; set; }
		public Entity ChiefAssistantTournamentDirector { get; set; }
		public Entity OtherTournamentDirectors { get; set; }
		public string Processed { get; set; }
		public int NumberOfSections { get; set; }
		public int Players { get; set; }
		public IEnumerable<SectionSummary> Sections { get; set; }
	}

	public class SectionSummary
	{
		public string Name { get; set; }
		public string SectionDates { get; set; }
		public string Processed { get; set; }
		public string Stats { get; set; }
		public IEnumerable<SectionResult> Results { get; set; }
	}

	public class SectionResult
	{
		public int Rank { get; set; }
		public string Name { get; set; }
		public string PlayerId { get; set; }
		public string State { get; set; }
		public float TotalPoints { get; set; }
		public string NValue { get; set; }
		public IEnumerable<Round> Rounds { get; set; }
		public IEnumerable<RatingChange> Ratings { get; set; }
	}

	public class Round
	{
		public int Number { get; set; }
		public string Outcome { get; set; }
		public int Versus { get; set; }
	}

	public class RatingChange
	{
		public string Type { get; set; }
		public string Before { get; set; }
		public string After { get; set; }
	}

	public class Entity
	{
		public string Id { get; set; }
		public string Name { get; set; }
	}
}