using System;
using System.Collections.Generic;

namespace JTC.SharpLinter
{
	public class LintDataCollector
	{
		private readonly bool _processUnuseds;

		public LintDataCollector(bool processUnuseds)
		{
			_processUnuseds = processUnuseds;
		}

		public List<JsLintData> Errors { get; } = new List<JsLintData>();
		public object predef { get; set; }

		public void ProcessData(object data)
		{
			var dataDict = data as Dictionary<string, object>;

			if (dataDict != null)
			{
				if (dataDict.ContainsKey("errors"))
				{
					ProcessListOfObject(dataDict["errors"], error =>
					{
						var jsError = new JsLintData();
						jsError.Source = "lint";
						if (error.ContainsKey("line"))
						{
							jsError.Line = (int) error["line"];
						}

						if (error.ContainsKey("character"))
						{
							jsError.Character = (int) error["character"];
						}

						if (error.ContainsKey("reason"))
						{
							jsError.Reason = (string) error["reason"];
						}

						Errors.Add(jsError);
					});
				}

				if (_processUnuseds && dataDict.ContainsKey("unused"))
				{
					var lastLine = -1;
					JsLintData jsError = null;
					var unusedList = String.Empty;
					var unusedCount = 0;
					ProcessListOfObject(dataDict["unused"], unused =>
					{
						var line = 0;
						if (unused.ContainsKey("line"))
						{
							line = (int) unused["line"];
						}
						if (line != lastLine)
						{
							if (jsError != null)
							{
								jsError.Reason = "Unused Variable" + (unusedCount > 1 ? "s " : " ") + unusedList;
								Errors.Add(jsError);
							}
							jsError = new JsLintData();
							jsError.Source = "lint";
							jsError.Character = -1;
							jsError.Line = line;
							unusedCount = 0;
							unusedList = String.Empty;
						}

						if (unused.ContainsKey("name"))
						{
							unusedList += (unusedCount == 0 ? String.Empty : ", ") + unused["name"];
							unusedCount++;
						}
						lastLine = line;
					});
					jsError.Reason = "Unused Variable" + (unusedCount > 1 ? "s " : " ") + unusedList;
					Errors.Add(jsError);
				}
			}
		}

		private void ProcessListOfObject(object obj, Action<Dictionary<string, object>> processor)
		{
			var array = obj as object[];

			if (array != null)
			{
				foreach (var objItem in array)
				{
					var objItemDictionary = objItem as Dictionary<string, object>;

					if (objItemDictionary != null)
					{
						processor(objItemDictionary);
					}
				}
			}
		}
	}
}