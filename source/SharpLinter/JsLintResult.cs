using System.Collections.Generic;

namespace SharpLinter
{
	/// <summary>
	/// Represents the result of linting some javascript
	/// </summary>
	public class JsLintResult
	{
		public List<JsLintData> Errors { get; set; }
		public bool Limited { get; set; }
	}
}