using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using UscParser.Infrastructure;
using UscParser.Models;

namespace UscParser.Controllers
{
	[RoutePrefix("EventSummary")]
    public class EventSummaryController : ApiController
    {
		[Route("{id?}")]
		public async Task<HttpResponseMessage> Get(string id)
		{
			var results = EventInfo.Load(id);

			return Request.CreateResponse(results);
		}
    }

	public static class EventInfo
	{
		public static EventSummary Load(string id)
		{
			var client = new HttpClient();
			var response = client.GetAsync($"http://www.uschess.org/msa/XtblMain.php?{id}.0").Result;
			var content = response.Content.ReadAsStringAsync().Result;
			var html = new HtmlDocument();

			html.LoadHtml(content);

			var root = html.DocumentNode;
			var eventSummary = LoadEventSummary(root);
			var eventSections = LoadEventSections(root);

			eventSummary.Sections = eventSections;

			Console.WriteLine(JsonConvert.SerializeObject(eventSummary, Formatting.Indented));

			return eventSummary;
		}

		private static IEnumerable<SectionSummary> LoadEventSections(HtmlNode root)
		{
			var eventSections = root.Descendants("pre");

			foreach (var r in eventSections)
			{
				var sectionContainer = r.ParentNode.ParentNode.ParentNode;

				//var sectionTitle = sectionContainer.Descendants().FirstOrDefault(w => w.Attributes["colspan"]?.Value == "3" && w.Attributes["align"]?.Value == "center" && w.Attributes["bgcolor"]?.Value == "DDDDFF");
				var sectionName = sectionContainer.Descendants().Where(w => w.InnerText.StartsWith("Section ")).FirstOrDefault();
				//Console.WriteLine(sectionName.InnerText);
				//Console.WriteLine(new string('-', 80));

				var sectionSummary = sectionContainer.Descendants().Where(w => w.InnerText.StartsWith("Section ")).FirstOrDefault().ParentNode.ParentNode.ParentNode.Descendants("tr");

				var i = 0;
				var sb = new StringBuilder();

				sb.Append($"\"Name\": \"{sectionName.InnerText}\",");

				foreach (var y in sectionSummary.Skip(1))
				{
					foreach (var x in y.Descendants("td"))
					{
						if (string.IsNullOrWhiteSpace(x.InnerText))
						{
							continue;
						}

						var value = x.InnerText.Trim().Replace("&nbsp;", null);

						if (i % 2 == 0)
						{
							//Console.Write($"\"{value}\":");
							sb.Append($"\"{value.FormatSectionInfoFieldTitle()}\":");
						}
						else
						{
							//Console.Write($"\"{value}\",");
							sb.Append($"\"{value}\",");
						}

						i++;
					}
				}

				var separator = new string(r.InnerText.Trim().TakeWhile(c => c != '\n').ToArray());

				var alreadySeenSeparator = 0;
				var players = new List<string>();
				var lineAccumulator = new StringBuilder();

				foreach (var line in r.InnerText.Trim().Split('\n'))
				{
					if (line == separator)
					{
						if (alreadySeenSeparator >= 3)
						{
							players.Add(lineAccumulator.ToString());
							lineAccumulator.Clear();
							//Console.WriteLine();
							//Console.WriteLine();
							continue;
						}

						alreadySeenSeparator++;
						continue;
					}

					if (alreadySeenSeparator < 3)
					{
						continue;
					}

					if (line.StartsWith("Note: "))
					{
						continue;
					}

					lineAccumulator.AppendLine(line);
					//Console.WriteLine(line);
				}

				sb.Append("\"Results\": [");

				foreach (var player in players)
				{
					sb.Append(FormatPlayerSectionScore(player));
				}

				sb.Append("],");

				var plainTextSection = $"{{ {sb} }}";
				var result = JsonConvert.DeserializeObject<SectionSummary>(plainTextSection);
				//Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));

				yield return result;
			}
		}

		private static string FormatPlayerSectionScore(string player)
		{
			var sb = new StringBuilder();
			var i = 0;
			var ratingsAccumulator = new List<RatingChange>();

			//Console.WriteLine("{");
			sb.Append("{");

			foreach (var line in player.Split('\n'))
			{
				var y = 0;

				// score line
				if (i == 0)
				{
					foreach (var item in line.Split('|'))
					{
						if (y == 0)
						{
							// place
							//Console.WriteLine($"\"Rank\": {item.Trim()},");
							sb.Append($"\"Rank\": {item.Trim()},");
						}
						else if (y == 1)
						{
							// name
							//Console.WriteLine($"\"Name\": \"{Inflector.Titleize(item.Trim())}\",");
							sb.Append($"\"Name\": \"{Inflector.Titleize(item.Trim())}\",");
						}
						else if (y == 2)
						{
							// total
							//Console.WriteLine($"\"TotalPoints\": {item.Trim()},");
							sb.Append($"\"TotalPoints\": {item.Trim()},");
						}
						else
						{
							if (y == 3)
							{
								//Console.WriteLine("\"Rounds\": [");
								sb.Append("\"Rounds\": [");
							}

							if (string.IsNullOrWhiteSpace(item.Trim()))
							{
								continue;
							}

							// round
							var split = item.Trim().ReduceWhitespace().Split(' ');

							if (split.Length > 1)
							{
								//Console.WriteLine($"{{ \"Number\": {y - 2}, \"Outcome\": \"{split[0]}\", \"Versus\": \"{split[1]}\" }},");
								sb.Append($"{{ \"Number\": {y - 2}, \"Outcome\": \"{split[0]}\", \"Versus\": \"{split[1]}\" }},");
							}
							else
							{
								//Console.WriteLine($"{{ \"Number\": {y - 2}, \"Outcome\": \"{split[0]}\", \"Versus\": \"{split[1]}\" }},");
								sb.Append($"{{ \"Number\": {y - 2}, \"Outcome\": \"{split[0]}\", \"Versus\": \"0\" }},");
							}
						}

						y++;
					}

					//Console.WriteLine("],");
					sb.Append("],");
				}

				y = 0;

				// ratings line
				if (i == 1)
				{
					foreach (var item in line.Split('|'))
					{
						if (string.IsNullOrWhiteSpace(item))
						{
							continue;
						}

						if (y == 0)
						{
							// state
							//Console.WriteLine($"\"State\": \"{item.Trim()}\",");
							sb.Append($"\"State\": \"{item.Trim()}\",");
						}
						else if (y == 1)
						{
							var split = item.Trim().Split('/');

							// player ID + R
							//Console.WriteLine($"\"PlayerId\": \"{split[0].Trim()}\",");
							sb.Append($"\"PlayerId\": \"{split[0].Trim()}\",");

							ratingsAccumulator.Add(split[1].FormatRatingChange());
						}
						else if (y == 2)
						{
							// N:1
							//Console.WriteLine($"\"NValue\": \"{item.Trim()}\",");
							sb.Append($"\"NValue\": \"{item.Trim()}\",");
						}

						y++;
					}
				}

				if (i > 1)
				{
					foreach (var item in line.Split('|'))
					{
						if (string.IsNullOrWhiteSpace(item))
						{
							continue;
						}

						ratingsAccumulator.Add(item.FormatRatingChange());
					}
				}

				i++;

				//Console.WriteLine(line);
			}

			//Console.WriteLine($"\"Ratings\": {JsonConvert.SerializeObject(ratingsAccumulator)},");
			sb.Append($"\"Ratings\": {JsonConvert.SerializeObject(ratingsAccumulator)},");

			//Console.WriteLine("},");
			sb.Append("},");
			//Console.WriteLine(new string('+', 80));

			return sb.ToString();
		}

		private static EventSummary LoadEventSummary(HtmlNode root)
		{
			var eventSummary = root.Descendants().Where(w => w.InnerText == "Event Summary").FirstOrDefault().ParentNode.ParentNode.Descendants("tr");

			var i = 0;
			var sb = new StringBuilder();

			foreach (var r in eventSummary)
			{
				foreach (var x in r.Descendants("td"))
				{
					if (string.IsNullOrWhiteSpace(x.InnerText))
					{
						continue;
					}

					var value = x.InnerText.Trim().Replace("&nbsp;", null);

					if (i % 2 == 0)
					{
						//Console.Write($"\"{value}\":");
						sb.Append($"\"{value.FormatEventInfoFieldTitle()}\":");
					}
					else
					{
						//Console.Write($"\"{value}\",");
						sb.Append($"\"{value}\",");
					}

					i++;
				}
			}

			var plainTextEventSummary = $"{{ {sb} }}";
			var result = JsonConvert.DeserializeObject<EventSummaryDto>(plainTextEventSummary);

			var model = new EventSummary();

			model.Event = result.Event.ToEntity();
			model.Location = result.Location.Titleize().FormatStateAbbreviation().Trim();
			model.EventDates = result.EventDates.Trim();
			model.SponsoringAffiliates = result.SponsoringAffiliates.ToEntity();
			model.ChiefTournamentDirector = result.ChiefTournamentDirector.ToEntity();
			model.ChiefAssistantTournamentDirector = result.ChiefAssistantTournamentDirector.ToEntity();
			model.OtherTournamentDirectors = result.OtherTournamentDirectors.ToEntity();
			model.Processed = result.Processed.Trim();

			var stats = result.Stats.Split('\n')[0].Split(',');
			model.NumberOfSections = int.Parse(stats[0].Replace(" Section(s)", null).Trim());
			model.Players = int.Parse(stats[1].Replace(" Players", null).Trim());

			//Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
			//Console.WriteLine(JsonConvert.SerializeObject(model, Formatting.Indented));

			return model;
		}
	}
}