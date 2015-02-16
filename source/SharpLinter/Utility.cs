using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Dynamic;

namespace SharpLinter
{
	public static class ExtensionMethods
	{
		/// <summary>
		/// Returns the text between startIndex and endIndex (exclusive of endIndex)
		/// </summary>
		/// <param name="text"></param>
		/// <param name="startIndex"></param>
		/// <param name="endIndex"></param>
		/// <returns></returns>
		public static string SubstringBetween(this string text, int startIndex, int endIndex)
		{
			return (text.Substring(startIndex, endIndex - startIndex));
		}

		public static string AfterLast(this string text, string find)
		{
			var index = text.LastIndexOf(find, StringComparison.Ordinal);
			if (index < 0 || index + find.Length >= text.Length)
			{
				return (String.Empty);
			}
			return (text.Substring(index + find.Length));
		}

		/// <summary>
		/// Removes /r /n /t and spaces
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string RemoveWhitespace(this string text)
		{
			var pos = -1;
			var len = text.Length;
			var removeList = new HashSet<char>("\r\n\t ");
			var output = new StringBuilder();
			while (++pos < len)
			{
				if (!removeList.Contains(text[pos]))
				{
					output.Append(text[pos]);
				}
			}
			return output.ToString();
		}

        public static Dictionary<string, object> ToDictionary(this ExpandoObject obj)
        {
            return (obj as IDictionary<string, object>).ToDictionary(x => x.Key, x => x.Value);
        }
    }

	public static class Utility
	{
		/// <summary>
		/// Resolves a path, using the application root as the path for nonrooted files
		/// </summary>
		/// <returns></returns>
		public static string ResolveRelativePath_AppRoot(string path)
		{
			return Path.IsPathRooted(path)
				? path
				: Assembly.GetExecutingAssembly().Location.BeforeLast("\\") + "\\" + path;
		}

		/// <summary>
		/// Qualifies a relative path file, using the current directory for non rooted files
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string ResolveRelativePath_Current(string path)
		{
			return Path.IsPathRooted(path)
				? path
				: Directory.GetCurrentDirectory() + "\\" + path;
		}

		/// <summary>
		/// Like IsTrueString, but if a true or false value is not matched, the default value is returned. The
		/// default can be null if no known
		/// true/false strings are matched.
		/// </summary>
		/// <param name="theString"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public static bool? StringToBool(string theString, bool? defaultValue)
		{
			var lcaseString = theString?.ToLower().Trim() ?? "";
			switch (lcaseString)
			{
				case "true":
				case "yes":
				case "y":
				case "1":
				case "on":
					return (true);
				case "false":
				case "no":
				case "n":
				case "0":
				case "off":
					return (false);
			}
			return (defaultValue);
		}

		public static string Before(this string text, string find)
		{
			var index = text.IndexOf(find, StringComparison.Ordinal);
			if (index < 0 || index == text.Length)
			{
				return (String.Empty);
			}
			return (text.Substring(0, index));
		}

		public static string BeforeLast(this string text, string find)
		{
			var index = text.LastIndexOf(find, StringComparison.Ordinal);
			return index >= 0 ? (text.Substring(0, index)) : String.Empty;
		}

		public static int OccurrencesOf(this string text, string find)
		{
			var finished = false;
			var pos = 0;
			var occurrences = 0;
			while (!finished)
			{
				pos = text.IndexOf(find, pos, StringComparison.Ordinal);
				if (pos >= 0)
				{
					occurrences++;
					pos++;
					if (pos == text.Length)
					{
						finished = true;
					}
				}
				else
				{
					finished = true;
				}
			}
			return occurrences;
		}

		public static string AddListItem(this string list, string value, string separator)
		{
			if (String.IsNullOrEmpty(value))
			{
				return list.Trim();
			}
			list = list?.Trim() ?? String.Empty;

			var pos = (list + separator).IndexOf(value + separator, StringComparison.Ordinal);
			if (pos >= 0) return (list);
			if (list.LastIndexOf(separator, StringComparison.Ordinal) == list.Length - separator.Length)
			{
				// do not add separator - it already exists
				return list + value;
			}
			return (list + (list == "" ? "" : separator) + value);
			// already has value
		}
    }
}