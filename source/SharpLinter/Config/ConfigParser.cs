using System;
using System.IO;
using Newtonsoft.Json;

namespace SharpLinter.Config
{
	internal class ConfigParser
	{
		public static LintConfig ParseLintFile(string filePath)
		{
			try
			{
				return JsonConvert.DeserializeObject<LintConfig>(File.ReadAllText(filePath));
			}
			catch (Exception ex)
			{
				throw new ArgumentException("Error parsing jshintrc configuration file at '" + filePath + "'. See exception log for details.", ex);
			}
		}
	}
}