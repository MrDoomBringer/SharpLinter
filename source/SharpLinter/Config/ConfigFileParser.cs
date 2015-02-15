using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpLinter.Config
{
	public class ConfigFileParser
	{
		private static readonly char[] StringSep = {','};
		public string ConfigData { get; set; }

		/// <summary>
		/// Parses key/value data of the format key:value, key2:value2. If value is missing, the string "true"
		/// is
		/// returned in its place, making this acceptable for mixed mode parsing.
		/// </summary>
		/// <param name="sectionName"></param>
		/// <returns></returns>
		public IEnumerable<KeyValuePair<string, string>> GetKVPSection(string sectionName)
		{
			var list = GetSection(sectionName).Split(StringSep, StringSplitOptions.RemoveEmptyEntries);
			return list.Select(item => item.Split(':')).Select(kvpArray => new KeyValuePair<string, string>(kvpArray[0].Trim(),
				kvpArray.Length > 1 ? kvpArray[1].Trim() : "true"));
		}

		public IEnumerable<string> GetValueSection(string sectionName, string separators)
		{
			var splitSeparators = separators.ToArray();

			var list = GetSection(sectionName).Split(splitSeparators, StringSplitOptions.RemoveEmptyEntries);
			return
				list.Select(item => item.Replace("\n", "").Replace("\r", "").Replace("\t", "").Trim())
					.Where(output => output != string.Empty);
		}

		private string GetSection(string sectionName)
		{
			var pos = ConfigData.IndexOf("/*" + sectionName, StringComparison.Ordinal);
			if (pos < 0)
			{
				return String.Empty;
			}

			var endPos = ConfigData.IndexOf(sectionName + "*/", pos, StringComparison.Ordinal);
			if (endPos < 0)
			{
				throw new Exception("Config section '" + sectionName + "' was not closed.");
			}
			return ConfigData.SubstringBetween(pos + sectionName.Length + 2, endPos);
		}
	}
}