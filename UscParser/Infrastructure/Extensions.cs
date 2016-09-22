using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using UscParser.Models;

namespace UscParser.Infrastructure
{
	public static class Extensions
	{
		public static Entity ToEntity(this string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return null;
			}

			try
			{
				return new Entity
				{
					Id = value
						.Substring(value.IndexOf('(') + 1, value.IndexOf(')') - value.IndexOf('(') - 1)
						.Trim(),
					Name = value
						.Replace($"({value.Substring(value.IndexOf('(') + 1, value.IndexOf(')') - value.IndexOf('(') - 1)})", null)
						.Trim()
						.Titleize()
				};
			}
			catch
			{
				return null;
			}
		}

		public static string FormatEventInfoFieldTitle(this string title)
		{
			if (string.IsNullOrWhiteSpace(title))
			{
				return null;
			}

			switch (title)
			{
				case "Event Date(s)":
					return "EventDates";
				case "Sponsoring Affiliate":
				case "Sponsoring Affiliates":
					return "SponsoringAffiliates";
				case "ChiefTD":
				case "Chief TD":
					return "ChiefTournamentDirector";
				case "ChiefAssist.TD":
					return "ChiefAssistantTournamentDirector";
				case "Other TDs":
					return "OtherTournamentDirectors";
				default:
					return title.Replace(" ", null);
			}
		}

		public static string FormatSectionInfoFieldTitle(this string title)
		{
			if (string.IsNullOrWhiteSpace(title))
			{
				return null;
			}

			switch (title)
			{
				case "Section Date(s)":
					return "SectionDates";
				default:
					return title.Replace(" ", null);
			}
		}

		public static RatingChange FormatRatingChange(this string value)
		{
			var ratingSplit = value.Replace("->", "|").Split('|');

			return new RatingChange
			{
				Type = value.Trim().Substring(0, 1).Trim(),
				Before = ratingSplit[0].Trim().Replace($"{value.Trim().Substring(0, 1).Trim()}: ", null).Trim(),
				After = ratingSplit[1].Trim()
			};
		}

		public static string ReduceWhitespace(this string value)
		{
			var newString = new StringBuilder();
			var previousIsWhitespace = false;

			for (int i = 0; i < value.Length; i++)
			{
				if (char.IsWhiteSpace(value[i]))
				{
					if (previousIsWhitespace)
					{
						continue;
					}

					previousIsWhitespace = true;
				}
				else
				{
					previousIsWhitespace = false;
				}

				newString.Append(value[i]);
			}

			return newString.ToString();
		}

		public static string FormatStateAbbreviation(this string value)
		{
			return value
				.Replace("Al ", "AL ")
				.Replace("Ak ", "AK ")
				.Replace("Az ", "AZ ")
				.Replace("Ar ", "AR ")
				.Replace("Ca ", "CA ")
				.Replace("Co ", "CO ")
				.Replace("Ct ", "CT ")
				.Replace("De ", "DE ")
				.Replace("Dc ", "DC ")
				.Replace("Fl ", "FL ")
				.Replace("Ga ", "GA ")
				.Replace("Hi ", "HI ")
				.Replace("Id ", "ID ")
				.Replace("Il ", "IL ")
				.Replace("In ", "IN ")
				.Replace("Ia ", "IA ")
				.Replace("Ks ", "KS ")
				.Replace("Ky ", "KY ")
				.Replace("La ", "LA ")
				.Replace("Me ", "ME ")
				.Replace("Md ", "MD ")
				.Replace("Ma ", "MA ")
				.Replace("Mi ", "MI ")
				.Replace("Mn ", "MN ")
				.Replace("Ms ", "MS ")
				.Replace("Mo ", "MO ")
				.Replace("Mt ", "MT ")
				.Replace("Ne ", "NE")
				.Replace("Nv ", "NV ")
				.Replace("Nh ", "NH ")
				.Replace("Nj ", "NJ ")
				.Replace("Nm ", "NM ")
				.Replace("Ny ", "NY ")
				.Replace("Nc ", "NC ")
				.Replace("Nd ", "ND ")
				.Replace("Oh ", "OH ")
				.Replace("Ok ", "OK ")
				.Replace("Or ", "OR ")
				.Replace("Pa ", "PA ")
				.Replace("Ri ", "RI ")
				.Replace("Sc ", "SC ")
				.Replace("Sd ", "SD ")
				.Replace("Tn ", "TN ")
				.Replace("Tx ", "TX ")
				.Replace("Ut ", "UT ")
				.Replace("Vt ", "VT ")
				.Replace("Va ", "VA ")
				.Replace("Wa ", "WA ")
				.Replace("Wv ", "WV ")
				.Replace("Wi ", "WI ")
				.Replace("Wy ", "WY ")
				.Replace("Usa", "USA");
		}
	}
}