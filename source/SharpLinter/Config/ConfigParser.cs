using System.IO;
using Newtonsoft.Json;

namespace SharpLinter.Config
{
	internal class ConfigParser
	{
		public ConfigParser(string filePath)
		{
			var reader = JsonConvert.DeserializeObject<LintConfig>(File.ReadAllText(filePath));
		}
	}
}