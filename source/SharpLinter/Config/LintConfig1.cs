using Newtonsoft.Json;

namespace SharpLinter.Config
{
	internal class LintConfig
	{
		/// <summary>
		/// JSON setting for herp.
		/// </summary>
		[JsonProperty("herp")]
		public string herp { get; set; }
	}
}
