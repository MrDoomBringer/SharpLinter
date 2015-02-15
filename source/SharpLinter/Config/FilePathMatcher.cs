using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SharpLinter.Config
{
	public static class FilePathMatcher
	{
		/// <summary>
		/// Returns only the files from names that match pattern, unless exclude, in which case only those that
		/// don't match
		/// </summary>
		/// <param name="pattern"></param>
		/// <param name="names"></param>
		/// <returns></returns>
		public static IEnumerable<string> MatchFiles(string pattern, IEnumerable<string> names, bool exclude)
		{
			string[] patterns = {pattern};
			return MatchFiles(patterns, names, exclude);
		}

		public static IEnumerable<string> MatchFiles(IEnumerable<string> patterns, IEnumerable<string> names, bool exclude)
		{
			Regex nameRegex = null;
			var match = false;

			foreach (var path in names)
			{
				var cleanPath = path.Replace("/", "\\");
				var fileNameOnly = FileNamePart(path);
				foreach (var pattern in patterns)
				{
					//noPatterns = false;
					var cleanPattern = pattern.Replace("/", "\\");
					var namePattern = FileNamePart(cleanPattern);
					var pathPattern = PathPart(cleanPattern);

					if (namePattern != String.Empty)
					{
						nameRegex = FindFilesPatternToRegex.Convert(namePattern);
					}


					if (nameRegex != null)
						match = (pathPattern == String.Empty || MatchPathOnly(cleanPattern, cleanPath)) &&
								(namePattern == String.Empty || nameRegex.IsMatch(fileNameOnly));
					if (match)
					{
						break;
					}
				}
				if (match != exclude)
				{
					yield return path;
				}
			}
		}

		private static string FileNamePart(string pattern)
		{
			return pattern.Substring(pattern.Length - 1, 1) == "\\"
				? String.Empty
				: (pattern.IndexOf("\\", StringComparison.Ordinal) == -1 ? pattern : pattern.AfterLast("\\"));
		}

		private static string PathPart(string pattern)
		{
			return pattern.Substring(pattern.Length - 1, 1) == "\\"
				? pattern
				: (pattern.IndexOf("\\", StringComparison.Ordinal) == -1 ? string.Empty : pattern.BeforeLast("\\"));
		}

		private static bool MatchPathOnly(string pattern, string path)
		{
			if (pattern.Substring(pattern.Length - 1, 1) != "\\")
			{
				return false;
			}
			if (pattern.IndexOf(":", StringComparison.Ordinal) > 0 || pattern.IndexOf("\\\\", StringComparison.Ordinal) == 0)
			{
				return path.StartsWith(pattern);
			}
			return path.IndexOf(pattern, StringComparison.Ordinal) >= 0;
		}

		private static class FindFilesPatternToRegex
		{
			private const string NonDotCharacters = @"[^.]*";
			private static readonly Regex HasQuestionMarkRegEx = new Regex(@"\?", RegexOptions.Compiled);
			private static Regex _hasAsteriskRegex = new Regex(@"\*", RegexOptions.Compiled);
			private static readonly Regex IlegalCharactersRegex = new Regex("[" + @"\/:<>|" + "\"]", RegexOptions.Compiled);
			private static readonly Regex CatchExtentionRegex = new Regex(@"^\s*.+\.([^\.]+)\s*$", RegexOptions.Compiled);

			public static Regex Convert(string pattern)
			{
				if (pattern == null)
				{
					throw new ArgumentNullException();
				}
				pattern = pattern.Trim();
				if (pattern.Length == 0)
				{
					throw new ArgumentException("Pattern is empty.");
				}
				if (IlegalCharactersRegex.IsMatch(pattern))
				{
					throw new ArgumentException("Patterns contains illegal characters.");
				}
				var hasExtension = CatchExtentionRegex.IsMatch(pattern);
				var matchExact = false;
				if (HasQuestionMarkRegEx.IsMatch(pattern))
				{
					matchExact = true;
				}
				else if (hasExtension)
				{
					matchExact = CatchExtentionRegex.Match(pattern).Groups[1].Length != 3;
				}
				var regexString = Regex.Escape(pattern);
				regexString = "^" + Regex.Replace(regexString, @"\\\*", ".*");
				regexString = Regex.Replace(regexString, @"\\\?", ".");
				if (!matchExact && hasExtension)
				{
					regexString += NonDotCharacters;
				}
				regexString += "$";
				var regex = new Regex(regexString, RegexOptions.Compiled | RegexOptions.IgnoreCase);
				return regex;
			}
		}
	}
}