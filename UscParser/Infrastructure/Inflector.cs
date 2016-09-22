using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UscParser.Infrastructure
{
	public static class Inflector
	{
		static Inflector()
		{
			AddPlural("$", "s");
			AddPlural("s$", "s");
			AddPlural("(ax|test)is$", "$1es");
			AddPlural("(octop|vir)us$", "$1i");
			AddPlural("(alias|status)$", "$1es");
			AddPlural("(bu)s$", "$1ses");
			AddPlural("(buffal|tomat)o$", "$1oes");
			AddPlural("([ti])um$", "$1a");
			AddPlural("sis$", "ses");
			AddPlural("(?:([^f])fe|([lr])f)$", "$1$2ves");
			AddPlural("(hive)$", "$1s");
			AddPlural("([^aeiouy]|qu)y$", "$1ies");
			AddPlural("(x|ch|ss|sh)$", "$1es");
			AddPlural("(matr|vert|ind)ix|ex$", "$1ices");
			AddPlural("([m|l])ouse$", "$1ice");
			AddPlural("^(ox)$", "$1en");
			AddPlural("(quiz)$", "$1zes");

			AddSingular("s$", "");
			AddSingular("(n)ews$", "$1ews");
			AddSingular("([ti])a$", "$1um");
			AddSingular("((a)naly|(b)a|(d)iagno|(p)arenthe|(p)rogno|(s)ynop|(t)he)ses$", "$1$2sis");
			AddSingular("(^analy)ses$", "$1sis");
			AddSingular("([^f])ves$", "$1fe");
			AddSingular("(hive)s$", "$1");
			AddSingular("(tive)s$", "$1");
			AddSingular("([lr])ves$", "$1f");
			AddSingular("([^aeiouy]|qu)ies$", "$1y");
			AddSingular("(s)eries$", "$1eries");
			AddSingular("(m)ovies$", "$1ovie");
			AddSingular("(x|ch|ss|sh)es$", "$1");
			AddSingular("([m|l])ice$", "$1ouse");
			AddSingular("(bus)es$", "$1");
			AddSingular("(o)es$", "$1");
			AddSingular("(shoe)s$", "$1");
			AddSingular("(cris|ax|test)es$", "$1is");
			AddSingular("(octop|vir)i$", "$1us");
			AddSingular("(alias|status)es$", "$1");
			AddSingular("^(ox)en", "$1");
			AddSingular("(vert|ind)ices$", "$1ex");
			AddSingular("(matr)ices$", "$1ix");
			AddSingular("(quiz)zes$", "$1");
		}

		private class Rule
		{
			private readonly Regex _regex;
			private readonly string _replacement;

			public Rule(string pattern, string replacement)
			{
				_regex = new Regex(pattern, RegexOptions.IgnoreCase);
				_replacement = replacement;
			}

			public string Apply(string word)
			{
				if (!_regex.IsMatch(word))
				{
					return null;
				}

				return _regex.Replace(word, _replacement);
			}
		}

		internal static void AddIrregular(string singular, string plural)
		{
			AddPlural(string.Format("({0}){1}$", singular[0], singular.Substring(1)), string.Format("$1{0}", plural.Substring(1)));
			AddSingular(string.Format("({0}){1}$", plural[0], plural.Substring(1)), string.Format("$1{0}", singular.Substring(1)));
		}

		internal static void AddUncountable(string word)
		{
			_uncountables.Add(word.ToLower());
		}

		internal static void AddPlural(string rule, string replacement)
		{
			_plurals.Add(new Rule(rule, replacement));
		}

		internal static void AddSingular(string rule, string replacement)
		{
			_singulars.Add(new Rule(rule, replacement));
		}

		private static readonly List<Rule> _plurals = new List<Rule>();
		private static readonly List<Rule> _singulars = new List<Rule>();
		private static readonly List<string> _uncountables = new List<string>();

		public static string Pluralize(string word)
		{
			return ApplyRules(_plurals, word);
		}

		public static string Singularize(string word)
		{
			return ApplyRules(_singulars, word);
		}

		private static string ApplyRules(List<Rule> rules, string word)
		{
			string result = word;

			if (!_uncountables.Contains(word.ToLower()))
				for (int i = rules.Count - 1; i >= 0; i--)
					if ((result = rules[i].Apply(word)) != null)
						break;

			return result;
		}

		public static string MatchUnits(int count, string word)
		{
			var matchedWord = Singularize(word) ?? word;
			return string.Format("{0} {1}", count, (count <= 1 && count >= -1) ? matchedWord : Pluralize(matchedWord));
		}

		public static string Titleize(this string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return text;
			}

			return Regex.Replace(
				Humanize(Underscore(text)),
				@"\b([a-z])",
				(match) =>
					match.Captures[0].Value.ToUpper()
			);
		}

		public static string Humanize(this string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return text;
			}

			return Capitalize(Regex.Replace(text, @"_", " "));
		}

		public static string Pascalize(this string text)
		{
			return Regex.Replace(
				text,
				"(?:^|_)(.)",
				(match) =>
					match.Groups[1].Value.ToUpper()
			);
		}

		public static string Camelize(this string text)
		{
			return Uncapitalize(Pascalize(text));
		}

		public static string Underscore(this string text)
		{
			return Regex.Replace(Regex.Replace(Regex.Replace(text, @"([A-Z]+)([A-Z][a-z])", "$1_$2"), @"([a-z\d])([A-Z])", "$1_$2"), @"[-\s]", "_").ToLower();
		}

		public static string Capitalize(this string text)
		{
			return text.Substring(0, 1).ToUpper() + text.Substring(1).ToLower();
		}

		public static string Uncapitalize(this string text)
		{
			return text.Substring(0, 1).ToLower() + text.Substring(1);
		}

		public static string Ordinalize(this string text)
		{
			int n = int.Parse(text);
			int mod = n % 100;

			if (mod >= 11 && mod <= 13)
				return text + "th";

			switch (n % 10)
			{
				case 1:
					return text + "st";
				case 2:
					return text + "nd";
				case 3:
					return text + "rd";
				default:
					return text + "th";
			}
		}

		public static string Dasherize(this string text)
		{
			return text.Replace('_', '-');
		}

		public static string Slugerize(this string text)
		{
			var str = text.ToLower();

			str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
			str = Regex.Replace(str, @"\s+", " ").Trim();
			str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
			str = Regex.Replace(str, @"\s", "-");

			return str;
		}

		public static string Ellipsize(this string text, int length, bool showMore = false)
		{
			if (string.IsNullOrWhiteSpace(text))
				return null;

			if (text.Length <= length)
				return text;

			var pos = text.IndexOf(" ", length);

			if (pos < 0)
				return text;

			return showMore ?
				string.Format("<div>{0}<span class=\"show-more\">...more</span><span class=\"show-more-text\">{1}</span></div>", text.Substring(0, pos), text.Substring(pos, text.Length - pos)) :
				string.Format("{0}...", text.Substring(0, pos));
		}

		public static string EllipsizeStrict(this string text, int length, bool showMore = false)
		{
			if (string.IsNullOrWhiteSpace(text))
				return null;

			if (text.Length <= length)
				return text;

			return showMore ?
				string.Format("<div>{0}<span class=\"show-more\">...more</span><span class=\"show-more-text\">{1}</span></div>", text.Substring(0, length), text.Substring(length, text.Length - length)) :
				string.Format("{0}...", text.Substring(0, length));
		}
	}
}